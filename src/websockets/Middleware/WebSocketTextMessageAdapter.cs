using System.Text;
using Faactory.Channels.Adapters;
using Faactory.Channels.Buffers;
using Faactory.Channels.Handlers;
using Microsoft.Extensions.Logging;

namespace Faactory.Channels.WebSockets.Adapters;

/// <summary>
/// This output adapter forwards strings to the channel's output pipeline as a text WebSocket message.
/// </summary>
internal sealed class WebSocketTextMessageAdapter( ILoggerFactory loggerFactory ) : ChannelAdapter<string>, IOutputChannelAdapter
{
    private readonly ILogger logger = loggerFactory.CreateLogger<WebSocketTextMessageAdapter>();

    public override Task ExecuteAsync( IAdapterContext context, string data )
    {
        /*
        ignore null or empty strings
        */
        if ( string.IsNullOrEmpty( data ) )
        {
            return Task.CompletedTask;
        }

        context.Forward( new WebSocketMessage
        {
            Type = System.Net.WebSockets.WebSocketMessageType.Text,
            Data = new WrappedByteBuffer( Encoding.UTF8.GetBytes( data ) )
        } );

        logger.LogDebug( "Forwarded WebSocketMessage with text data." );

        return Task.CompletedTask;
    }
}
