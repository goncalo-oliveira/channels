namespace Faactory.Channels.Buffers;

/// <summary>
/// A checkpoint interface for readable byte buffers,
/// allowing to mark a position in the buffer before doing speculative reads.
/// </summary>
public interface IReadableCheckpoint : IDisposable
{
    /// <summary>
    /// Gets the underlying buffer associated with this checkpoint
    /// </summary>
    IReadableByteBuffer Buffer { get; }

    /// <summary>
    /// Updates the offset of the underlying buffer to reflect the read operations
    /// performed through this checkpoint's buffer.
    /// </summary>
    void Commit();
}
