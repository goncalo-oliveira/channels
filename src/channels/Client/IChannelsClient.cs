namespace Faactory.Channels.Client;

/// <summary>
/// A Channels client
/// </summary>
public interface IChannelsClient : IDisposable
{
    /// <summary>
    /// The channel to use for communication
    /// </summary>
    IChannel Channel { get; }

    /// <summary>
    /// Closes the channel and releases all resources. This method should be used over Channel.CloseAsync when the client is no longer needed.
    /// </summary>
    Task CloseAsync();
}
