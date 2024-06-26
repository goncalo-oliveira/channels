namespace Faactory.Channels;

/// <summary>
/// Channel event listener
/// </summary>
public interface IChannelMonitor
{
    /// <summary>
    /// Triggered when the channel is created
    /// </summary>
    void ChannelCreated( IChannelInfo channelInfo );

    /// <summary>
    /// Triggered when the channel is closed
    /// </summary>
    void ChannelClosed( IChannelInfo channelInfo );

    /// <summary>
    /// Triggered when data is received by the channel
    /// </summary>
    void DataReceived( IChannelInfo channelInfo, byte[] data );

    /// <summary>
    /// Triggered when data is sent by the channel
    /// </summary>
    void DataSent( IChannelInfo channelInfo, int sent );

    /// <summary>
    /// Triggered when a custom event is raised
    /// </summary>
    void CustomEvent( IChannelInfo channelInfo, string name, object? data );
}
