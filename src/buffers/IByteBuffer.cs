namespace Faactory.Channels.Buffers;

/// <summary>
/// A buffer handling interface
/// </summary>
[Serialization.ByteBufferJsonConverter]
public interface IByteBuffer
{
    /// <summary>
    /// Gets the endianness of the buffer
    /// </summary>
    public Endianness Endianness { get; }

    /// <summary>
    /// Gets the length of the buffer
    /// </summary>
    int Length { get; }

    /// <summary>
    /// Gets the full logical contents of the buffer as a span.
    /// </summary>
    /// <returns>A <see cref="ReadOnlySpan{T}"/> containing the full logical contents of the buffer.</returns>
    ReadOnlySpan<byte> AsSpan();

    /// <summary>
    /// Gets the full logical contents of the buffer as a byte array.
    /// </summary>
    /// <returns>A byte array containing the full logical contents of the buffer.</returns>
    byte[] ToArray();
}
