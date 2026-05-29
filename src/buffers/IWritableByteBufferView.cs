namespace Faactory.Channels.Buffers;

/// <summary>
/// A buffer view that provides write capabilities while sharing the same underlying memory as the original buffer.
/// </summary>
public interface IWritableByteBufferView : IWritableByteBuffer
{
    /// <summary>
    /// Gets the number of bytes that can be written to the view before reaching its limit.
    /// </summary>
    int WritableBytes { get; }
}
