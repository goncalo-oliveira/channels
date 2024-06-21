using Faactory.Channels.WebSockets;

namespace Microsoft.Extensions.DependencyInjection;

public static class WebSocketChannelServiceExtensions
{
    /// <summary>
    /// Adds the WebSocket channels middleware to the service collection and returns a builder for configuring named channels.
    /// </summary>
    public static INamedWebSocketChannelBuilder AddWebSocketChannels( this IServiceCollection services )
    {
        services.AddTransient<IWebSocketChannelFactory, WebSocketChannelFactory>();

        return new NamedWebSocketChannelBuilder( services );
    }

    /// <summary>
    /// Adds the WebSocket channels middleware to the service collection and returns a builder for configuring named channels.
    /// </summary>
    /// <param name="configure">A delegate that configures the default channel.</param>
    public static INamedWebSocketChannelBuilder AddWebSocketChannels( this IServiceCollection services, Action<IWebSocketChannelBuilder> configure )
    {
        services.AddTransient<IWebSocketChannelFactory, WebSocketChannelFactory>();

        var builder = new NamedWebSocketChannelBuilder( services )
            .AddChannel( WebSocketChannelFactory.DefaultChannelName, configure );

        return builder;
    }
}
