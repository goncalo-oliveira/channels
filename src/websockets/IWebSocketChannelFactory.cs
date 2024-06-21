namespace Faactory.Channels.WebSockets;

public interface IWebSocketChannelFactory
{
    /// <summary>
    /// Creates a new WebSocket channel instance using the specified WebSocket connection.
    /// </summary>
    /// <param name="webSocket">The WebSocket connection to use for the channel.</param>
    /// <param name="name">The name of the channel to create. If not specified, the default channel name will be used.</param>
    IWebSocketChannel CreateChannel( System.Net.WebSockets.WebSocket webSocket, string? name = null );
}
