namespace Faactory.Channels;

/// <summary>
/// Defines a contract for limiting the number of concurrent channel connections.
/// </summary>
public interface IChannelLimiter
{
    /// <summary>
    /// Determines whether a given channel is admitted based on the current connection limit.
    /// </summary>
    /// <param name="channel">The channel to check for admission.</param>
    /// <returns>True if the channel is admitted; otherwise, false.</returns>
    public bool IsAdmitted( IChannel channel );
}
