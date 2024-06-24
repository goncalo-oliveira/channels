using Faactory.Channels.WebSockets;

namespace Microsoft.Extensions.DependencyInjection;

public static class WebSocketChannelServiceExtensions
{
    /// <summary>
    /// Adds the WebSocket channels middleware to the service collection and returns a builder for configuring named channels.
    /// </summary>
    public static IServiceCollection AddWebSocketChannels( this IServiceCollection services )
    {
        services.AddTransient<IWebSocketChannelFactory, WebSocketChannelFactory>();

        return services;
    }
}
