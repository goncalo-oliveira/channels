using Faactory.Channels;
using Faactory.Channels.Handlers;

/*
This handler does nothing... it exists only to consume the data received from the pipeline.
*/

public class VoidHandler : ChannelHandler<byte[]>
{
    public override Task ExecuteAsync( IChannelContext context, byte[] data )
    {
        return Task.CompletedTask;
    }
}
