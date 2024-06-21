namespace Faactory.Channels.Client;

public sealed class ClientChannelOptions : ChannelOptions
{
    /// <summary>
    /// Gets or sets the transport mode. Default is TCP.
    /// </summary>
    public ChannelTransportMode TransportMode { get; set; } = ChannelTransportMode.Tcp;
}
