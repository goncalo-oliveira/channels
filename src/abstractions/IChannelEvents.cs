namespace Faactory.Channels;

/// <summary>
/// Channel event listener
/// </summary>
public interface IChannelEvents
{
    void ChannelCreated( IChannelInfo channelInfo );
    void ChannelClosed( IChannelInfo channelInfo );
    void DataReceived( IChannelInfo channelInfo, byte[] data );
    void DataSent( IChannelInfo channelInfo, int sent );
}
