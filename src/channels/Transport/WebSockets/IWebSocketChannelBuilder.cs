namespace Faactory.Channels.WebSockets;

public interface IWebSocketChannelBuilder : IChannelBuilder<IWebSocketChannelBuilder>
{
    IWebSocketChannelBuilder Configure( Action<ChannelOptions> configure );
}
