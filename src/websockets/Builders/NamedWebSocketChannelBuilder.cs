using Faactory.Channels.WebSockets.Adapters;
using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels.WebSockets;

public interface INamedWebSocketChannelBuilder
{
    IServiceCollection Services { get; }

    INamedWebSocketChannelBuilder AddChannel( string name, Action<IWebSocketChannelBuilder> configure );
}

internal sealed class NamedWebSocketChannelBuilder( IServiceCollection services ) : INamedWebSocketChannelBuilder
{
    public IServiceCollection Services { get; } = services;

    public INamedWebSocketChannelBuilder AddChannel( string name, Action<IWebSocketChannelBuilder> configure )
    {
        var channelBuilder = new WebSocketChannelBuilder( Services, name );

        // the WebSocketTextMessageAdapter is a special adapter that converts WebSocket messages to byte[] content
        channelBuilder.AddOutputAdapter<WebSocketTextMessageAdapter>();

        configure?.Invoke( channelBuilder );

        return this;
    }
}
