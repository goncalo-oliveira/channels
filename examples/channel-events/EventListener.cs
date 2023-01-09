using Faactory.Channels;
using Microsoft.Extensions.Logging;

public class EventListener : IChannelEvents
{
    private readonly ILogger logger;

    public EventListener( ILogger<EventListener> logger )
    {
        this.logger = logger;
    }

    public void ChannelCreated( IChannelInfo channelInfo )
    {
        logger.LogInformation( $"Channel created." );
    }

    public void ChannelClosed( IChannelInfo channelInfo )
    {
        logger.LogInformation( $"Channel closed." );
    }

    public void DataReceived( IChannelInfo channelInfo, byte[] data )
    {
        logger.LogInformation( $"Channel received {data.Length} bytes." );
    }

    public void DataSent( IChannelInfo channelInfo, int sent )
    {
        logger.LogInformation( $"Channel sent {sent} bytes." );
    }
}
