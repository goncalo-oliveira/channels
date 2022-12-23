namespace Faactory.Channels;

/// <summary>
/// A long-running service with the same lifespan of a channel
/// </summary>
public interface IChannelService : IDisposable
{
    /// <summary>
    /// Starts the service. Invoked when a channel is created.
    /// </summary>
    void Start( IChannel channel );

    /// <summary>
    /// Stops the service. Invoked when a channel is closed.
    /// </summary>
    void Stop();
}
