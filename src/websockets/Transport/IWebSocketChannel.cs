namespace Faactory.Channels.WebSockets;

/// <summary>
/// A WebSocket channel
/// </summary>
public interface IWebSocketChannel
{
    /// <summary>
    /// Gets the identifier of the channel
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Closes the channel
    /// </summary>
    Task CloseAsync();

    /// <summary>
    /// Waits for the channel to close or the cancellation token to be triggered
    /// </summary>
    Task WaitAsync( CancellationToken cancellationToken = default );
}
