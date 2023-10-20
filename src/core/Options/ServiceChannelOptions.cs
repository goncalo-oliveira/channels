namespace Faactory.Channels;

/// <summary>
/// Channel options for hosted service
/// </summary>
public class ServiceChannelOptions : ChannelOptions
{
    /// <summary>
    /// Gets or sets the service listning port. Default is 8080.
    /// </summary>
    public int Port { get; set; } = 8080;

    /// <summary>
    /// Gets or sets the maximum length of the pending connections queue. Default is 100.
    /// </summary>
    public int Backlog { get; set; } = 100;
}
