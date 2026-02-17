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
    private readonly Task initializeTask;
    private Task idleMonitorTask = Task.CompletedTask;

    public Channel( IServiceScope serviceScope )
    {
        ChannelScope = serviceScope;
        Info = new ChannelInfo( this );

        logger = serviceScope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<Channel>();

        initializeTask = InitializeAsync( cts.Token );
            
        _ = initializeTask.ContinueWith(
            t => logger.LogError( t.Exception?.GetBaseException(), "Init failed" ),
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default
        );

        logger.LogTrace( "Created" );
    }

    internal IChannelInfo Info { get; }
    internal IServiceProvider ServiceProvider => ChannelScope.ServiceProvider;

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

    public Endianness BufferEndianness => Buffer.Endianness;

    public IChannelPipeline Input { get; protected set; } = EmptyChannelPipeline.Instance;
    public IChannelPipeline Output { get; protected set; } = EmptyChannelPipeline.Instance;

    public string Id { get; } = Guid.NewGuid().ToString( "N" );

    private volatile bool isClosed;
    public bool IsClosed { get => isClosed; private set => isClosed = value; }

    public IWritableByteBuffer Buffer { get; protected set; } = new WritableByteBuffer();

    public ChannelData Data { get; protected set; } = [];

    public DateTimeOffset Created { get; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? LastReceived { get; private set; }

    public DateTimeOffset? LastSent { get; private set; }

    protected IEnumerable<IChannelService> Services { get; init; } = [];

    public TimeSpan Timeout { get; protected set; } = TimeSpan.FromSeconds( 60 );

    public IChannelService? GetChannelService( Type serviceType )
        => Services.SingleOrDefault( s => s.GetType() == serviceType );

    private int isClosing;
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
        if ( initializeTask != null )
        {
            try
            {
                await initializeTask
                    .ConfigureAwait( false );
            }
            catch { }
        }

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

    public virtual void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();

        GC.SuppressFinalize( this );
    }

    public virtual async ValueTask DisposeAsync()
    {
        await CloseAsync()
            .ConfigureAwait( false );

        Input.Dispose();
        Output.Dispose();

        ChannelScope.Dispose();

        GC.SuppressFinalize( this );
    }

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
        => ServiceProvider.GetServices<IChannelMonitor>()
            .InvokeAll( x => x.ChannelCreated( Info ) );

    private void NotifyChannelClosed()
        => ServiceProvider.GetServices<IChannelMonitor>()
            .InvokeAll( x => x.ChannelClosed( Info ) );

    /// <summary>
    /// Notifies the channel monitors that data has been received. This method should be called by derived classes when data is received.
    /// </summary>
    /// <param name="data">The data that was received.</param>
    protected void NotifyDataReceived( byte[] data )
    {
        LastReceived = DateTimeOffset.UtcNow;

        ServiceProvider.GetServices<IChannelMonitor>()
            .InvokeAll( x => x.DataReceived( Info, data ) );
    }

    /// <summary>
    /// Notifies the channel monitors that data has been sent. This method should be called by derived classes when data is sent.
    /// </summary>
    /// <param name="data">The data that was sent.</param>
    protected void NotifyDataSent( byte[] data )
    {
        LastSent = DateTimeOffset.UtcNow;

        ServiceProvider.GetServices<IChannelMonitor>()
            .InvokeAll( x => x.DataSent( Info, data ) );
    }

    /// <summary>
    /// Notifies the channel monitors of a custom event. This method should be called by derived classes when a custom event occurs.
    /// </summary>
    /// <param name="name">The name of the custom event.</param>
    /// <param name="data">The data associated with the custom event.</param>
    public void NotifyCustomEvent( string name, object? data )
        => ServiceProvider.GetServices<IChannelMonitor>()
            .InvokeAll( x => x.CustomEvent( Info, name, data ) );

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
