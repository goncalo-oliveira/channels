namespace Faactory.Channels;

/// <summary>
/// Options for the channel connection limiter.
/// </summary>
public sealed class ChannelLimiterOptions
{
    /// <summary>
    /// The maximum number of concurrent connections allowed. Set to 0 for unlimited.
    /// </summary>
    public int ConnectionLimit { get; set; }
}
