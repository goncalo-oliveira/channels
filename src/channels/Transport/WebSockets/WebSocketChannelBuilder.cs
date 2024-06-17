using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels.WebSockets;

internal sealed class WebSocketChannelBuilder( IServiceCollection services ) : ChannelBuilder<IWebSocketChannelBuilder>( services ), IWebSocketChannelBuilder
{
    public IWebSocketChannelBuilder Configure( Action<ChannelOptions> configure )
    {
        Services.Configure( configure );

        return Self();
    }

    protected override IWebSocketChannelBuilder Self() => this;
}
