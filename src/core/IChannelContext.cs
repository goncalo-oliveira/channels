using Faactory.Channels.Buffers;

namespace Faactory.Channels;

/// <summary>
/// A channel context used in middleware
/// </summary>
public interface IChannelContext
{
    /// <summary>
    /// Gets a buffer pool instance
    /// </summary>
    IByteBufferPool BufferPool { get; }

    /// <summary>
    /// Gets the channel instance
    /// </summary>
    IChannel Channel { get; }

    /// <summary>
    /// Gets the channel's buffer endianness
    /// </summary>
    Buffers.Endianness BufferEndianness { get; }

    /// <summary>
    /// Gets the output buffer
    /// </summary>
    IWritableBuffer Output { get; }

    /// <summary>
    /// Notifies a custom event to the channel
    /// </summary>
    /// <param name="name">The event name</param>
    /// <param name="data">The event data</param>
    void NotifyCustomEvent( string name, object? data = null );
}
