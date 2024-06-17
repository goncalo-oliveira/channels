namespace Faactory.Channels.WebSockets;

public interface IWebSocketChannelFactory
{
    Task<IWebSocketChannel> CreateChannelAsync( System.Net.WebSockets.WebSocket webSocket );
}
