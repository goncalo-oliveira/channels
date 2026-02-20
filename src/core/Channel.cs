using Faactory.Channels.Buffers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Faactory.Channels;

/// <summary>
/// An abstract class that represents a communication channel.
/// </summary>
public abstract class Channel : IChannel, IAsyncDisposable
{
    private readonly ILogger logger;
    private readonly CancellationTokenSource cts = new();
    private readonly Lazy<IChannelMonitor[]> monitors;
    private Task initializeTask = Task.CompletedTask;
    private Task idleMonitorTask = Task.CompletedTask;

    /// <summary>
    /// Initializes a new instance of the <see cref="Channel"/> class with the specified service scope.
    /// </summary>
    /// <param name="serviceScope">The service scope for the channel. This scope is used to resolve services that are specific to the channel, such as channel adapters and handlers.</param>
    public Channel( IServiceScope serviceScope )
    {
        ChannelScope = serviceScope;
        Info = new ChannelInfo( this );

        logger = serviceScope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<Channel>();

        monitors = new( () => ServiceProvider.GetServices<IChannelMonitor>().ToArray() );

        logger.LogTrace( "Created" );
    }

    internal IChannelInfo Info { get; }
    internal IServiceProvider ServiceProvider => ChannelScope.ServiceProvider;

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
            t => logger.LogError( t.Exception?.GetBaseException(), "Init failed" ),
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
    public DateTimeOffset? LastReceived { get; private set; }

    /// <summary>
    /// The timestamp when the channel last sent data.
    /// </summary>
    public DateTimeOffset? LastSent { get; private set; }

    /// <summary>
    /// The channel services associated with the channel.
    /// </summary>
    protected IEnumerable<IChannelService> Services { get; init; } = [];

    /// <summary>
    /// A timeout value that determines how long the channel can be idle (i.e., without sending or receiving data) before it is automatically closed.
    /// </summary>
    public TimeSpan Timeout { get; protected set; } = TimeSpan.FromSeconds( 60 );

    /// <summary>
    /// Gets a channel service of the specified type.
    /// </summary>
    public IChannelService? GetChannelService( Type serviceType )
        => Services.SingleOrDefault( s => s.GetType() == serviceType );

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

        logger.LogInformation( "Closed." );

        // if idle monitor task is running, wait for it to complete
        try
        {
            await idleMonitorTask
                .ConfigureAwait( false );
        }
        catch { }

        // if initialization is still running, wait for it to complete
        try
        {
            await initializeTask
                .ConfigureAwait( false );
        }
        catch { }

        // notify channel closed
        try
        {
            NotifyChannelClosed();
        }
        catch { }

        logger.LogDebug( "Stopping services." );

        // stop channel services
        try
        {
            await StopServicesAsync()
                .ConfigureAwait( false );
        }
        catch { }

        logger.LogDebug( "Stopped services." );
    }

    /// <summary>
    /// Disposes the channel and releases all resources.
    /// </summary>
    public virtual void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();

        GC.SuppressFinalize( this );
    }

    /// <summary>
    /// Asynchronously disposes the channel and releases all resources.
    /// </summary>
    public virtual async ValueTask DisposeAsync()
    {
        await CloseAsync()
            .ConfigureAwait( false );

        Input.Dispose();
        Output.Dispose();

        ChannelScope.Dispose();

        GC.SuppressFinalize( this );
    }

    /// <summary>
    /// Writes data to the output pipeline. This method executes the output pipeline and sends the processed data to the underlying transport.
    /// </summary>
    public virtual async Task WriteAsync( object data )
    {
        if ( IsClosed )
        {
            logger.LogWarning( "Can't write to a closed channel." );

            return;
        }

        logger.LogDebug( "Executing output pipeline..." );

        await Output.ExecuteAsync( this, data, LifetimeToken )
            .ConfigureAwait( false );
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

        logger.LogDebug( "Initialized" );
    }

    private void NotifyChannelCreated()
    {
        Metrics.ActiveChannels.Add( 1 );

        monitors.Value.InvokeAll( x => x.ChannelCreated( Info ) );
    }

    private void NotifyChannelClosed()
    {
        Metrics.ActiveChannels.Add( -1 );

        monitors.Value.InvokeAll( x => x.ChannelClosed( Info ) );
    }

    /// <summary>
    /// Notifies the channel monitors that data has been received. This method should be called by derived classes when data is received.
    /// </summary>
    /// <param name="data">The data that was received.</param>
    protected void NotifyDataReceived( ReadOnlySpan<byte> data )
    {
        LastReceived = DateTimeOffset.UtcNow;

        Metrics.BytesReceived.Add( data.Length );

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
        LastSent = DateTimeOffset.UtcNow;

        Metrics.BytesSent.Add( data.Length );

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
        var tasks = Services.Select( async service =>
        {
            try
            {
                await service.StartAsync( this, cancellationToken );
            }
            catch ( Exception ex )
            {
                logger.LogError(
                    ex,
                    "Failed to start '{TypeName}' channel service. {Message}",
                    service.GetType().Name,
                    ex.Message
                );
            }
        } );

        return Task.WhenAll( tasks );
    }

    private Task StopServicesAsync()
    {
        var tasks = Services.Select( async service =>
        {
            try
            {
                await service.StopAsync();
            }
            catch ( Exception ex )
            {
                logger.LogError(
                    ex,
                    "Failed to stop '{TypeName}' channel service. {Message}",
                    service.GetType().Name,
                    ex.Message
                );
            }
        } );

        return Task.WhenAll( tasks );
    }

    private async Task MonitorIdleStateAsync( CancellationToken cancellationToken )
    {
        LastReceived = DateTimeOffset.UtcNow;
        LastSent = DateTimeOffset.UtcNow;

        logger.LogDebug( "Idle state monitoring started." );

        while ( !cancellationToken.IsCancellationRequested )
        {
            try
            {
                await Task.Delay( 1000, cancellationToken );

                var ts = LastReceived > LastSent ? LastReceived : LastSent;

                if ( ts?.Add( Timeout ) < DateTimeOffset.UtcNow )
                {
                    logger.LogWarning(
                        "Channel has been idle for more than {seconds} seconds.",
                        (int)Timeout.TotalSeconds
                    );

                    await CloseAsync()
                        .ConfigureAwait( false );

                    Metrics.IdleTimeouts.Add( 1 );

                    break;
                }
            }
            catch ( OperationCanceledException )
            {
                break;
            }
        }

        logger.LogDebug( "Idle state monitoring stopped." );
    }
}
