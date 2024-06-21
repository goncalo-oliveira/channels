using System.Text;
using Faactory.Channels.Adapters;

namespace Faactory.Channels.Examples;

/*
This adapters receives a string and encodes it as an UTF8 buffer.

Take note that this adapter is an `IOutputChannelAdapter`.
*/

public class UTFEncoderAdapter : ChannelAdapter<string>, IOutputChannelAdapter
{
    //
    public override Task ExecuteAsync( IAdapterContext context, string data )
    {
        var bytes = Encoding.UTF8.GetBytes( data );

        // this is what will be sent back to the channel
        context.Forward( bytes );

        return Task.CompletedTask;
    }
}
