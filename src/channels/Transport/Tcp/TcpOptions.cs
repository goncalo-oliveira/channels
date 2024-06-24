namespace Faactory.Channels.Tcp;

public sealed class TcpChannelListenerOptions
{
    public int Port { get; set; } = 8080;
    public int Backlog { get; set; } = 100;
}
