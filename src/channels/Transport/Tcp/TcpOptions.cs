namespace Faactory.Channels.Tcp;

/// <summary>
/// Options for TCP channel listener.
/// </summary>
public sealed class TcpChannelListenerOptions
{
    /// <summary>
    /// The port number to listen on. Default is 8080.
    /// </summary>
    public int Port { get; set; } = 8080;

    /// <summary>
    /// The maximum length of the pending connections queue. Default is 100.
    /// </summary>
    public int Backlog { get; set; } = 100;
}
