using Microsoft.Extensions.Logging;

namespace Faactory.Channels.Adapters;

internal class OutputChannelAdapter : ChannelAdapter<byte[]>
{
    private readonly ILogger logger;

    public OutputChannelAdapter( ILoggerFactory loggerFactory )
    {
        logger = loggerFactory.CreateLogger<OutputChannelAdapter>();
    }

    public override Task ExecuteAsync( IAdapterContext context, byte[] data )
    {
        ((Sockets.ConnectedSocket)context.Channel).Send( data );

        logger.LogDebug( $"Written {data.Length} bytes to the channel." );

        return Task.CompletedTask;
    }
}
