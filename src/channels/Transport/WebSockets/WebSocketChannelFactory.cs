using System.Net.WebSockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Faactory.Channels.WebSockets;

internal sealed class WebSocketChannelFactory( IServiceProvider serviceProvider, IOptions<ChannelOptions> optionsAccessor ) : IWebSocketChannelFactory
{
    private readonly IServiceProvider provider = serviceProvider;
    private readonly ChannelOptions options = optionsAccessor.Value;

   public async Task<IWebSocketChannel> CreateChannelAsync( WebSocket webSocket )
    {
        var scope = provider.CreateScope();

        var options = scope.ServiceProvider.GetRequiredService<IOptions<ChannelOptions>>();

        var channel = new WebSocketChannel(
            scope,
            webSocket,
            options.Value.BufferEndianness
        );

        await channel.InitializeAsync();

        return channel;
    }
}
