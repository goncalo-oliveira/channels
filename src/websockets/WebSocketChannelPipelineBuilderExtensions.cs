using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Faactory.Channels.WebSockets;

internal static class WebSocketChannelPipelineBuilderExtensions
{
    public static ChannelPipelineBuilder AddWebSocketsMiddleware( this ChannelPipelineBuilder pipelineBuilder )
    {
        var loggerFactory = pipelineBuilder.PipelineServices.GetRequiredService<ILoggerFactory>();

        // the WebSocketTextMessageAdapter is a special adapter that converts WebSocket messages to byte[] content
        pipelineBuilder.AddOutputAdapter( new Adapters.WebSocketTextMessageAdapter( loggerFactory ) );

        // add the specialized handlers
        pipelineBuilder.AddOutputHandler( new Handlers.WebSockets.WebSocketChannelHandler( loggerFactory ) ); // handle WebSocket-specific messages
        pipelineBuilder.AddOutputHandler( new Handlers.OutputChannelHandler( loggerFactory ) );               // handle byte[] content (sent as binary data)

        return pipelineBuilder;
    }
}
