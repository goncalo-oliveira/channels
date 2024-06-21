using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels.WebSockets;

internal sealed class WebSocketChannelBuilder( IServiceCollection services, string channelName ) : ChannelBuilder<IWebSocketChannelBuilder>( services, channelName ), IWebSocketChannelBuilder
{
    public IWebSocketChannelBuilder Configure( Action<ChannelOptions> configure )
    {
        Services.Configure( Name, configure );

        return Self();
    }

    protected override IWebSocketChannelBuilder Self() => this;
}
