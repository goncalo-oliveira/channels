namespace Faactory.Channels;

/// <summary>
/// A channel context used in middleware
/// </summary>
public interface IChannelContext
{
    /// <summary>
    /// Gets the channel instance
    /// </summary>
    IChannel Channel { get; }

    /// <summary>
    /// Gets the output buffer
    /// </summary>
    IWritableBuffer Output { get; }

    void NotifyCustomEvent( string name, object? data = null );
}
