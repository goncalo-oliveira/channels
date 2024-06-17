using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using Faactory.Channels.Adapters;
using Faactory.Channels.WebSockets;

namespace Faactory.Channels.Examples;

/*
This adapters reads from a web socket message and counts the number of words.
It then forwards a structure with that information for the handler to deal with.

Notice we are using `WebSocketMessage` as the adapter type. On web socket channels,
this is the type of message that is sent to the pipeline.

The adapter also checks that the message type is text before processing it. Binary messages
are not supported by this adapter.

The word matching algorithm uses a regular expression.

Take note that this adapter is an `IInputChannelAdapter`.
*/

public partial class WordAdapter( ILoggerFactory loggerFactory ) : ChannelAdapter<WebSocketMessage>, IInputChannelAdapter
{
    private readonly ILogger logger = loggerFactory.CreateLogger<WordAdapter>();

    public override Task ExecuteAsync( IAdapterContext context, WebSocketMessage data )
    {
        if ( data.Type != WebSocketMessageType.Text )
        {
            logger.LogWarning( "Received a binary message. Skipping." );

            /*
            If we had middleware that could handle binary messages, we would forward the message
            to the next middleware in the pipeline. Since we don't, we just return and interrupt
            the pipeline.
            */

            return Task.CompletedTask;
        }

        /*
        Text messages are (usually) UTF-8 encoded.
        */
        var phrase = Encoding.UTF8.GetString( data.Data.ToArray() );

        var matches = MyRegex().Matches( phrase );

        // we forward the matches found
        context.Forward( matches );

        return Task.CompletedTask;
    }

    [GeneratedRegex("[\\S]+")]
    private static partial Regex MyRegex();
}
