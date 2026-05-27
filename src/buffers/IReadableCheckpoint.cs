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

/// <summary>
/// Extension methods for IReadableByteBuffer to create checkpoints for speculative reads.
/// </summary>
public static class ReadableByteBufferCheckpointExtensions
{
    /// <summary>
    /// Creates a checkpoint for the current buffer position for speculative reads.
    /// The buffer returned is a view of the original buffer, sharing the same underlying data but with an independent offset.
    /// </summary>
    /// <returns>A readable checkpoint instance</returns>
    public static IReadableCheckpoint Checkpoint( this IReadableByteBuffer source )
        => new ReadableCheckpoint( source );

}
