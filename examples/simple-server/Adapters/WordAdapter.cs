using System.Text;
using System.Text.RegularExpressions;
using Faactory.Channels.Adapters;

/*
This adapters reads from a buffer and counts the number of words.
It then forwards a structure with that information for the handler to deal with.

We are using `byte[]` as the adapter type since we are reading the entire buffer at once.
When dealing with binary protocols we can benefit from using `IByteBuffer` instead.

The word matching algorithm uses a regular expression.

Take note that this adapter is an `IInputChannelAdapter`.
*/

public class WordAdapter : ChannelAdapter<byte[]>, IInputChannelAdapter
{
    public override Task ExecuteAsync( IAdapterContext context, byte[] data )
    {
        var phrase = Encoding.UTF8.GetString( data );

        var matches = Regex.Matches( phrase, "[\\S]+" );

        // we forward the matches found
        context.Forward( matches );

        return Task.CompletedTask;
    }
}
