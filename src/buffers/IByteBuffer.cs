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
    /// Gets the entire buffer as a byte[] no matter where the reading/writing offset is
    /// </summary>
    /// <returns>A byte[] value</returns>
    byte[] ToArray();

    ReadOnlySpan<byte> AsSpan();
}
