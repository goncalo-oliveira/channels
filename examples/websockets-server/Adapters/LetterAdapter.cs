using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using Faactory.Channels.Adapters;
using Faactory.Channels.WebSockets;

namespace Faactory.Channels.Examples;

/*
This adapters reads from a web socket message.

After reading the message, it extracts all the letters of the alphabet from the message.
It then forwards the letters to the next middleware in the pipeline.

Take note that this adapter is an `IInputChannelAdapter`.
*/

public partial class LetterAdapter( ILoggerFactory loggerFactory ) : ChannelAdapter<WebSocketMessage>, IInputChannelAdapter
{
    private readonly ILogger logger = loggerFactory.CreateLogger<LetterAdapter>();

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
        var text = Encoding.UTF8.GetString( data.Data.ToArray() )
            .ToUpperInvariant();

        /*
        We use a regular expression to extract all the letters of the alphabet from the message.
        */
        var letters = MyRegex().Matches( text )
            .Select( m => m.Value.First() )
            .Distinct()
            .OrderBy( c => c )
            .ToArray();

        context.Forward( letters );

        return Task.CompletedTask;
    }

    [GeneratedRegex("[A-Z]")]
    private static partial Regex MyRegex();
}
