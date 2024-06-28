using Faactory.Channels.Buffers;
using Faactory.Channels.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Faactory.Channels;

/// <summary>
/// An abstract class that represents a communication channel.
/// </summary>
internal abstract class Channel : IChannel
{
    private readonly ILogger logger;
    private Task? monitorTask;

    public Channel( IServiceScope serviceScope )
    {
        ChannelScope = serviceScope;
        Info = new ChannelInfo( this );

        logger = serviceScope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<Channel>();

        logger.LogTrace( "Created" );
    }

    internal IChannelInfo Info { get; }
    internal IServiceProvider ServiceProvider => ChannelScope.ServiceProvider;

    protected IServiceScope ChannelScope { get; }

    public IChannelPipeline Input { get; protected set; } = EmptyChannelPipeline.Instance;
    public IChannelPipeline Output { get; protected set; } = EmptyChannelPipeline.Instance;

    public string Id { get; } = Guid.NewGuid().ToString( "N" );

    public bool IsClosed { get; protected set; }

    public IByteBuffer Buffer { get; protected set; } = new WritableByteBuffer();

    public ChannelData Data { get; protected set; } = [];

    public DateTimeOffset Created { get; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? LastReceived { get; protected set; }

    public DateTimeOffset? LastSent { get; protected set; }

    public IEnumerable<IChannelService> Services { get; init; } = [];

    public TimeSpan Timeout { get; protected set; } = TimeSpan.FromSeconds( 60 );

    public abstract Task CloseAsync();

    public virtual void Dispose()
    {
        // if monitor task is running, wait for it to complete
        monitorTask?.WaitForCompletion();
        monitorTask?.TryDispose();

        Input.Dispose();
        Output.Dispose();

        logger.LogDebug( "Disposed" );

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

        await Output.ExecuteAsync( this, data )
            .ConfigureAwait( false );
    }

    public abstract Task WriteRawBytesAsync( byte[] data );

    /// <summary>
    /// Initializes the channel. This method is called when the channel is created.
    /// </summary>
    protected virtual async Task InitializeAsync( CancellationToken cancellationToken = default )
    {
        // notify channel created
        this.NotifyChannelCreated();

        /*
        Start monitoring the channel if a timeout is set.
        */
        if ( Timeout > TimeSpan.Zero )
        {
            monitorTask = MonitorAsync( cancellationToken );
        }

        // start long-running services
        await StartServicesAsync( cancellationToken );

        logger.LogDebug( "Initialized" );
    }

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

    protected Task StopServicesAsync()
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

    private async Task MonitorAsync( CancellationToken cancellationToken )
    {
        LastReceived = DateTimeOffset.UtcNow;
        LastSent = DateTimeOffset.UtcNow;

        logger.LogDebug( "Monitoring started." );

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

        logger.LogDebug( "Monitoring stopped." );
    }
}
