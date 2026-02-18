namespace Faactory.Channels.Udp;

/// <summary>
/// Options for a UDP channel listener.
/// </summary>
public sealed class UdpChannelListenerOptions
{
    /// <summary>
    /// The port to listen on. Default is 8080.
    /// </summary>
    public int Port { get; set; } = 8080;
}
