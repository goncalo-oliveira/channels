namespace Parcel.Channels;

/// <summary>
/// Idle channel detection mode
/// </summary>
public enum IdleDetectionMode
{
    /// <summary>
    /// No idle channel detection
    /// </summary>
    None = -1,

    /// <summary>
    /// Automatic detection (default)
    /// </summary>
    Auto,

    /// <summary>
    /// Applies a hard timeout on (not) received data
    /// </summary>
    Read,

    /// <summary>
    /// Applies a hard timeout on (not) sent data
    /// </summary>
    Write,

    /// <summary>
    /// Applies a hard timeout to both (not) received and (not) sent data
    /// </summary>
    Both
}
