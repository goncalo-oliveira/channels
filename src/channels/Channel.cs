using Faactory.Channels.Buffers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Faactory.Channels;

/// <summary>
/// An abstract class that represents a communication channel.
/// </summary>
internal abstract class Channel : IChannel
{
    private readonly ILogger logger;

    public Channel( IServiceScope serviceScope )
    {
        ChannelScope = serviceScope;
        Info = new ChannelInfo( this );

        logger = serviceScope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<Channel>();
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

    public abstract Task CloseAsync();

    public virtual void Dispose()
    {
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

        await Output.ExecuteAsync( this, data )
            .ConfigureAwait( false );
    }

    public abstract Task WriteRawBytesAsync( byte[] data );

    /// <summary>
    /// Initializes the channel. This method is called when the channel is created.
    /// </summary>
    protected virtual async Task InitializeAsync( CancellationToken cancellationToken = default )
    {
        logger.LogInformation( "Created" );

        // notify channel created
        this.NotifyChannelCreated();

        // start long-running services
        await StartServicesAsync( cancellationToken );
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
}
