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

    /// <summary>
    /// The policy to apply when the connection limit is reached.
    /// Defaults to <see cref="ConnectionLimitPolicy.RejectNewest"/>.
    /// </summary>
    public ConnectionLimitPolicy ConnectionLimitPolicy { get; set; } = ConnectionLimitPolicy.RejectNewest;
}
