namespace Faactory.Channels;

/// <summary>
/// Channel options for client
/// </summary>
public class ClientChannelOptions : ChannelOptions
{
    /// <summary>
    /// Gets or sets the service host name. Default is localhost.
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// Gets or sets the service host port. Default is 8080.
    /// </summary>
    public int Port { get; set; } = 8080;
}
