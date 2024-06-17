using Faactory.Channels.WebSockets;
using Microsoft.Extensions.Logging;

namespace Faactory.Channels.Handlers.WebSockets;

internal sealed class WebSocketChannelHandler( ILoggerFactory loggerFactory ) : ChannelHandler<WebSocketMessage>
{
    private readonly ILogger logger = loggerFactory.CreateLogger<WebSocketChannelHandler>();

    public override async Task ExecuteAsync( IChannelContext context, WebSocketMessage message )
    {
        if ( context.Channel is WebSocketChannel wsChannel )
        {
            await wsChannel.WriteMessageAsync( message );

            logger.LogDebug( "Written {length} bytes to the channel.", message.Data.Length );
        }
    }
}
