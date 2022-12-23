namespace Faactory.Channels;

/// <summary>
/// Channel options for client
/// </summary>
public class ClientChannelOptions : ChannelOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 8080;
}
