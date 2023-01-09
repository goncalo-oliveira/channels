using Faactory.Channels;
using Faactory.Channels.Handlers;

/*
Since there are no adapters, the data gets to the handler as bytes.
We write the same data to the output buffer, to be sent at the end of the pipeline.
*/

public class EchoHandler : ChannelHandler<byte[]>
{
    public override Task ExecuteAsync( IChannelContext context, byte[] data )
    {
        context.Output.Write( data );

        return Task.CompletedTask;
    }
}
