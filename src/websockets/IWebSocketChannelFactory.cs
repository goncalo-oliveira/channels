namespace Faactory.Channels.WebSockets;

public interface IWebSocketChannelFactory
{
    /// <summary>
    /// Creates a new WebSocket channel instance using the specified WebSocket connection. Optionally, a custom channel pipeline builder can be provided.
    /// </summary>
    Task<IWebSocketChannel> CreateChannelAsync( System.Net.WebSockets.WebSocket webSocket, IChannelPipelineBuilder? pipelineBuilder = null );
}
