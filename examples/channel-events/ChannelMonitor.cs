using Microsoft.Extensions.Logging;

namespace Faactory.Channels.Examples;

public class ChannelMonitor( ILogger<ChannelMonitor> logger ) : IChannelEvents
{
    private readonly ILogger logger = logger;

    public void ChannelCreated( IChannelInfo channelInfo )
    {
        logger.LogInformation( "Channel created." );
    }

    public void ChannelClosed( IChannelInfo channelInfo )
    {
        logger.LogInformation( "Channel closed." );
    }

    public void DataReceived( IChannelInfo channelInfo, byte[] data )
    {
        logger.LogInformation(
            "Channel received {Length} bytes.",
            data.Length
        );
    }

    public void DataSent( IChannelInfo channelInfo, int sent )
    {
        logger.LogInformation(
            "Channel sent {sent} bytes.",
            sent
        );
    }

    public void CustomEvent( IChannelInfo channelInfo, string name, object? data )
    {
        logger.LogInformation(
            "Channel received custom event {name}.",
            name
        );
    }
}
