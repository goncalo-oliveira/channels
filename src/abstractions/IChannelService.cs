namespace Faactory.Channels;

/// <summary>
/// A long-running service with the same lifespan of a channel
/// </summary>
public interface IChannelService : IDisposable
{
    /// <summary>
    /// Starts the service. Invoked when a channel is created.
    /// </summary>
    Task StartAsync( IChannel channel, CancellationToken cancellationToken = default );

    /// <summary>
    /// Stops the service. Invoked when a channel is closed.
    /// </summary>
    Task StopAsync( CancellationToken cancellationToken = default );
}
