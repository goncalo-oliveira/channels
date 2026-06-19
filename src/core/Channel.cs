using System.Diagnostics;
using Faactory.Channels.Buffers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Faactory.Channels;

/// <summary>
/// An abstract class that represents a communication channel.
/// </summary>
public abstract class Channel : IChannel, IAsyncDisposable
{
    private readonly ILogger logger;
    private readonly CancellationTokenSource cts = new();
    private readonly IChannelRegistrar registrar;
    private readonly Lazy<IChannelMonitor[]> monitors;
    private readonly Func<IChannelInfo, TagList> metricsTagsFactory;
    private Task initializeTask = Task.CompletedTask;
    private Task idleMonitorTask = Task.CompletedTask;
    private long lastReceived;
    private long lastSent;

    /// <summary>
    /// Initializes a new instance of the <see cref="Channel"/> class with the specified service scope.
    /// </summary>
    /// <param name="serviceScope">The service scope for the channel. This scope is used to resolve services that are specific to the channel, such as channel adapters and handlers.</param>
    public Channel( IServiceScope serviceScope )
    {
        ChannelScope = serviceScope;
        Info = new ChannelInfo( this );

        logger = Services.GetRequiredService<ILoggerFactory>()
            .CreateLogger<Channel>();

        registrar = Services.GetService<IChannelRegistrar>()
            ?? NullChannelRegistrar.Instance;

        monitors = new( () => Services.GetServices<IChannelMonitor>().ToArray() );

        metricsTagsFactory = Services.GetRequiredService<IOptions<ChannelOptions>>().Value.MetricsTagsFactory;

        ScopeLogger(
            logger => logger.LogDebug( "Created" )
        );
    }

    internal IChannelInfo Info { get; }

    internal TagList GetMetricsTags() => metricsTagsFactory( Info );

    private int initializeStarted;

    /// <summary>
    /// Begins the asynchronous initialization of the channel.
    /// This method should be called by derived classes when the channel is created.
    /// </summary>
    protected void BeginInitialize()
    {
        if ( Interlocked.Exchange( ref initializeStarted, 1 ) == 1 )
        {
            return;
        }

        initializeTask = InitializeAsync( cts.Token );
            
        _ = initializeTask.ContinueWith(
            t => ScopeLogger( logger => logger.LogError( t.Exception?.GetBaseException(), "Init failed. {Message}", t.Exception?.GetBaseException().Message ) ),
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default
        );
    }

    /// <summary>
    /// The service scope for the channel.
    /// </summary>
    /// <remarks>
    /// This can be used to resolve services that are specific to the channel, such as channel adapters and handlers.
    /// The channel scope is disposed when the channel is disposed, so any services resolved from the channel scope will also be disposed when the channel is disposed.
    /// </remarks>
    protected IServiceScope ChannelScope { get; }

    /// <summary>
    /// A cancellation token that is triggered when the channel is closed.
    /// </summary>
    /// <remarks>
    /// This can be used by derived classes to cancel any ongoing operations when the channel is closed.
    /// </remarks>
    protected CancellationToken LifetimeToken => cts.Token;

    /// <summary>
    /// Gets the scoped service provider associated with the channel lifetime.
    /// Services resolved from this provider are scoped to the current channel instance.
    /// </summary>
    public IServiceProvider Services => ChannelScope.ServiceProvider;

    /// <summary>
    /// The pipeline for processing incoming data.
    /// </summary>
    public IChannelPipeline Input { get; protected set; } = EmptyChannelPipeline.Instance;

    /// <summary>
    /// The pipeline for processing outgoing data.
    /// </summary>
    public IChannelPipeline Output { get; protected set; } = EmptyChannelPipeline.Instance;

    /// <summary>
    /// The unique identifier for the channel.
    /// </summary>
    public string Id { get; } = Guid.NewGuid().ToString( "N" );

    /// <summary>
    /// The name of the channel, which corresponds to the channel configuration name.
    /// </summary>
    public string Name { get; protected set; } = string.Empty;

    private volatile bool isClosed;

    /// <summary>
    /// Gets whether the channel is closed. Once a channel is closed, it cannot be reopened.
    /// </summary>
    public bool IsClosed { get => isClosed; private set => isClosed = value; }

    /// <summary>
    /// A buffer for storing unprocessed incoming data.
    /// </summary>
    public IWritableByteBuffer Buffer { get; protected set; } = new WritableByteBuffer();

    /// <summary>
    /// The data associated with the channel.
    /// </summary>
    public ChannelData Data { get; protected set; } = [];

    /// <summary>
    /// The timestamp when the channel was created.
    /// </summary>
    public DateTimeOffset Created { get; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// The timestamp when the channel last received data.
    /// </summary>
    public DateTimeOffset? LastReceived => lastReceived > 0
        ? DateTimeOffset.FromUnixTimeMilliseconds( Interlocked.Read( ref lastReceived ) )
        : null;

    /// <summary>
    /// The timestamp when the channel last sent data.
    /// </summary>
    public DateTimeOffset? LastSent => lastSent > 0
        ? DateTimeOffset.FromUnixTimeMilliseconds( Interlocked.Read( ref lastSent ) )
        : null;

    /// <summary>
    /// The channel services initialized for the current channel instance.
    /// </summary>
    public IEnumerable<IChannelService> ChannelServices { get; init; } = [];

    /// <summary>
    /// A timeout value that determines how long the channel can be idle (i.e., without sending or receiving data) before it is automatically closed.
    /// </summary>
    public TimeSpan Timeout { get; protected set; } = TimeSpan.FromSeconds( 60 );

    private int isClosing;

    /// <summary>
    /// Closes the channel. Once a channel is closed, it cannot be reopened.
    /// </summary>
    public virtual async Task CloseAsync()
    {
        if ( Interlocked.Exchange( ref isClosing, 1 ) == 1 )
        {
            return;
        }

        IsClosed = true;

        try
        {
            cts.Cancel();
        }
        catch { }

        ScopeLogger(
            logger => logger.LogInformation( "Closed." )
        );

        // notify channel closed
        try
        {
            NotifyChannelClosed();
        }
        catch { }


        ScopeLogger(
            logger => logger.LogDebug( "Stopping services." )
        );

        // stop channel services
        try
        {
            await StopServicesAsync()
                .ConfigureAwait( false );
        }
        catch { }

        ScopeLogger(
            logger => logger.LogDebug( "Stopped services." )
        );
    }

    /// <summary>
    /// Disposes the channel and releases all resources.
    /// </summary>
    public void Dispose()
    {
        Dispose( true );

        GC.SuppressFinalize( this );
    }

    /// <summary>
    /// Asynchronously disposes the channel and releases all resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore()
            .ConfigureAwait( false );

        Dispose( true );

        GC.SuppressFinalize( this );
    }

    private int disposed;
    /// <summary>
    /// Performs the dispose operations for the channel.
    /// Override this method in derived classes to perform any additional cleanup operations when the channel is disposed.
    /// </summary>
    protected virtual void Dispose( bool disposing )
    {
        if ( Interlocked.Exchange( ref disposed, 1 ) == 1 )
        {
            return;
        }

        if ( !disposing )
        {
            return;
        }

        try
        {
            Input.Dispose();
        }
        catch { }

        try
        {
            Output.Dispose();
        }
        catch { }

        try
        {
            ChannelScope.Dispose();
        }
        catch { }
    }

    /// <summary>
    /// Performs the asynchronous dispose operations for the channel.
    /// Override this method in derived classes to perform any additional asynchronous cleanup operations when the channel is disposed.
    /// </summary>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        await CloseAsync()
            .ConfigureAwait( false );

        // wait for any pending operations to complete
        try
        {
            await Task.WhenAll( idleMonitorTask, initializeTask )
                .ConfigureAwait( false );
        }
        catch { }
    }

    /// <summary>
    /// Writes data to the output pipeline. This method executes the output pipeline and sends the processed data to the underlying transport.
    /// </summary>
    public virtual async Task WriteAsync( object data )
    {
        if ( IsClosed )
        {
            ScopeLogger(
                logger => logger.LogWarning( "Can't write to a closed channel." )
            );

            return;
        }

        ScopeLogger(
            logger => logger.LogDebug( "Executing output pipeline..." )
        );

        try
        {
            await Output.ExecuteAsync( this, data, LifetimeToken )
                .ConfigureAwait( false );
        }
        catch ( OperationCanceledException )
        {
            // Expected shutdown
            return;
        }
    }

    /// <summary>
    /// Writes raw bytes to the underlying transport. This method should be implemented by derived classes to send data to the remote endpoint. The data passed to this method is the output of the output pipeline.
    /// </summary>
    /// <param name="data">The data to write to the underlying transport, after being processed by the output pipeline.</param>
    public abstract Task WriteRawBytesAsync( byte[] data );

    /// <summary>
    /// Initializes the channel. This method is called when the channel is created.
    /// </summary>
    protected virtual async Task InitializeAsync( CancellationToken cancellationToken = default )
    {
        // notify channel created
        NotifyChannelCreated();

        // start long-running services
        await StartServicesAsync( cancellationToken );

        // start monitor task if timeout is set
        if ( Timeout > TimeSpan.Zero )
        {
            idleMonitorTask = MonitorIdleStateAsync( cts.Token );
        }

        ScopeLogger(
            logger => logger.LogDebug( "Initialized" )
        );
    }

    private void NotifyChannelCreated()
    {
        Metrics.ActiveChannels.Add( 1 );

        registrar.Register( this );
        monitors.Value.InvokeAll( x => x.ChannelCreated( Info ) );
    }

    private void NotifyChannelClosed()
    {
        Metrics.ActiveChannels.Add( -1 );
        Metrics.ChannelDuration.Record( ( DateTimeOffset.UtcNow - Created ).TotalMilliseconds, GetMetricsTags() );

        monitors.Value.InvokeAll( x => x.ChannelClosed( Info ) );
    }

    /// <summary>
    /// Notifies the channel monitors that data has been received. This method should be called by derived classes when data is received.
    /// </summary>
    /// <param name="data">The data that was received.</param>
    protected void NotifyDataReceived( ReadOnlySpan<byte> data )
    {
        Interlocked.Exchange( ref lastReceived, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() );

        Metrics.DataReceived.Add( data.Length, GetMetricsTags() );

        foreach ( var service in monitors.Value )
        {
            service.DataReceived( Info, data );
        }
    }

    /// <summary>
    /// Notifies the channel monitors that data has been sent. This method should be called by derived classes when data is sent.
    /// </summary>
    /// <param name="data">The data that was sent.</param>
    protected void NotifyDataSent( ReadOnlySpan<byte> data )
    {
        Interlocked.Exchange( ref lastSent, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() );

        Metrics.DataSent.Add( data.Length, GetMetricsTags() );

        foreach ( var service in monitors.Value )
        {
            service.DataSent( Info, data );
        }
    }

    /// <summary>
    /// Notifies the channel monitors of a custom event. This method should be called by derived classes when a custom event occurs.
    /// </summary>
    /// <param name="name">The name of the custom event.</param>
    /// <param name="data">The data associated with the custom event.</param>
    public void NotifyCustomEvent( string name, object? data )
        => monitors.Value.InvokeAll( x => x.CustomEvent( Info, name, data ) );

    private Task StartServicesAsync( CancellationToken cancellationToken = default )
    {
        var tasks = ChannelServices.Select( async service =>
        {
            try
            {
                await service.StartAsync( this, cancellationToken );
            }
            catch ( Exception ex )
            {
                ScopeLogger(
                    logger => logger.LogError(
                        ex,
                        "Failed to start '{TypeName}' channel service. {Message}",
                        service.GetType().Name,
                        ex.Message
                    )
                );
            }
        } );

        return Task.WhenAll( tasks );
    }

    private Task StopServicesAsync()
    {
        var tasks = ChannelServices.Select( async service =>
        {
            try
            {
                await service.StopAsync();
            }
            catch ( Exception ex )
            {
                ScopeLogger(
                    logger => logger.LogError(
                        ex,
                        "Failed to stop '{TypeName}' channel service. {Message}",
                        service.GetType().Name,
                        ex.Message
                    )
                );
            }
        } );

        return Task.WhenAll( tasks );
    }

    private async Task MonitorIdleStateAsync( CancellationToken cancellationToken )
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        Interlocked.Exchange( ref lastReceived, now );
        Interlocked.Exchange( ref lastSent, now );

        ScopeLogger(
            logger => logger.LogDebug( "Idle state monitoring started." )
        );

        while ( !cancellationToken.IsCancellationRequested )
        {
            try
            {
                await Task.Delay( 1000, cancellationToken );

                var ts = LastReceived > LastSent ? LastReceived : LastSent;

                if ( ts?.Add( Timeout ) < DateTimeOffset.UtcNow )
                {
                    ScopeLogger(
                        logger => logger.LogWarning(
                            "Channel has been idle for more than {seconds} seconds.",
                            (int)Timeout.TotalSeconds
                        )
                    );

                    Metrics.IdleTimeouts.Add( 1, GetMetricsTags() );

                    await CloseAsync()
                        .ConfigureAwait( false );

                    break;
                }
            }
            catch ( OperationCanceledException )
            {
                break;
            }
        }

        ScopeLogger(
            logger => logger.LogDebug( "Idle state monitoring stopped." )
        );
    }

    /// <summary>
    /// Logs a message using the channel's logger with a scope that includes the channel ID.
    /// This method can be used by derived classes to log messages with the channel ID included in the log context.
    /// </summary>
    /// <param name="logAction">The action that performs the logging.</param>
    protected void ScopeLogger( Action<ILogger> logAction )
    {
        using var _ = logger.BeginScope( $"tcp-{Id[..7]}" );

        logAction( logger );
    }
}
