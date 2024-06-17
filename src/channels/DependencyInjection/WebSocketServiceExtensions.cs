using Faactory.Channels;
using Faactory.Channels.WebSockets;

namespace Microsoft.Extensions.DependencyInjection;

public static class WebSocketChannelServiceExtensions
{
    public static IServiceCollection AddWebSocketChannels( this IServiceCollection services, Action<IWebSocketChannelBuilder> configure )
    {
        services.AddTransient<IWebSocketChannelFactory, WebSocketChannelFactory>();

        configure( new WebSocketChannelBuilder( services ) );

        return services;
    }
}
