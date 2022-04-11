using Microsoft.Extensions.Logging;

namespace Faactory.Channels.Handlers;

internal class OutputChannelHandler : ChannelHandler<byte[]>
{
    private readonly ILogger logger;

    public OutputChannelHandler( ILoggerFactory loggerFactory )
    {
        logger = loggerFactory.CreateLogger<OutputChannelHandler>();
    }

    public override Task ExecuteAsync( IChannelContext context, byte[] data )
    {
        ((Sockets.ConnectedSocket)context.Channel).Send( data );

        logger.LogDebug( $"Written {data.Length} bytes to the channel." );

        return Task.CompletedTask;
    }
}
