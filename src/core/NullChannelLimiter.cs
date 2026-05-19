namespace Faactory.Channels;

/// <summary>
/// A channel limiter that does not impose any limits on the number of concurrent connections.
/// </summary>
public sealed class NullChannelLimiter : IChannelLimiter
{
    /// <summary>
    /// A singleton instance of the <see cref="NullChannelLimiter"/> class that can be used
    /// whenever a channel limiter is required but no limiting is desired.
    /// </summary>
    public static readonly IChannelLimiter Instance = new NullChannelLimiter();

    /// <summary>
    /// Determines whether a given channel is admitted. Always returns true, allowing all channels to be admitted.
    /// </summary>
    /// <param name="channel">The channel to check for admission.</param>
    /// <returns>Always returns true, indicating that the channel is admitted.</returns>
    public bool IsAdmitted( IChannel channel ) => true;
}
