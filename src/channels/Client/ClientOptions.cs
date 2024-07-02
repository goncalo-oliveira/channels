namespace Faactory.Channels.Client;

/// <summary>
/// Channels client options
/// </summary>
public class ChannelsClientOptions
{
    /// <summary>
    /// Gets or sets the service host name. Default is localhost.
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// Gets or sets the service host port. Default is 8080.
    /// </summary>
    public int Port { get; set; } = 8080;

    /// <summary>
    /// Gets or sets the transport mode. Default is TCP.
    /// </summary>
    public ChannelTransportMode TransportMode { get; set; } = ChannelTransportMode.Tcp;

    /// <summary>
    /// Gets or sets the reconnect delay after a connection is lost. Default is 3 seconds.
    /// </summary>
    public TimeSpan ReconnectDelay { get; set; } = TimeSpan.FromSeconds( 3 );

    /// <summary>
    /// Gets or sets the maximum reconnect delay in case of multiple connection losses. Default is 30 seconds.
    /// </summary>
    public TimeSpan MaxReconnectDelay { get; set; } = TimeSpan.FromSeconds( 30 );

    /// <summary>
    /// Gets or sets the channel options
    /// </summary>
    public ChannelOptions ChannelOptions { get; set; } = new();
}
