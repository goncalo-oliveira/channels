namespace Faactory.Channels;

/// <summary>
/// Defines policies for handling new channel connections when the connection limit is reached.
/// </summary>
public enum ConnectionLimitPolicy
{
    /// <summary>
    /// Rejects the newest connection attempt when the connection limit is reached.
    /// </summary>
    RejectNewest,

    /// <summary>
    /// Evicts the oldest active connection to make room for the new connection when the connection limit is reached.
    /// </summary>
    EvictOldest
}
