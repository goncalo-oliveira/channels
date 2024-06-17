using Microsoft.Extensions.Logging;

namespace Faactory.Channels.Handlers;

internal sealed class OutputChannelHandler( ILoggerFactory loggerFactory ) : ChannelHandler<byte[]>
{
    private readonly ILogger logger = loggerFactory.CreateLogger<OutputChannelHandler>();

    public override async Task ExecuteAsync( IChannelContext context, byte[] data )
    {
        if ( context.Channel is Channel channel )
        {
            await channel.WriteRawBytesAsync( data );

            logger.LogDebug( "Written {length} bytes to the channel.", data.Length );
        }
    }
}
