namespace Faactory.Channels.Hosting;

/// <summary>
/// Channel options for hosted service
/// </summary>
public class ServiceChannelOptions : ChannelOptions
{
    public int Port { get; set; } = 8080;
    public int Backlog { get; set; } = 100;
}
