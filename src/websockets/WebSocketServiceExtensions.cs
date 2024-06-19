using Faactory.Channels;
using Faactory.Channels.WebSockets;

namespace Microsoft.Extensions.DependencyInjection;

public static class WebSocketChannelServiceExtensions
{
    public static IServiceCollection AddWebSocketChannels( this IServiceCollection services, Action<IWebSocketChannelBuilder>? configure = null )
    {
        services.AddTransient<IWebSocketChannelFactory, WebSocketChannelFactory>();

        configure?.Invoke( new WebSocketChannelBuilder( services ) );
        // configure?.Invoke( new WebSocketChannelBuilder( "__default", services ) );

        return services;
    }
}
