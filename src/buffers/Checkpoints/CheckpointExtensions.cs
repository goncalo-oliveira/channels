namespace Faactory.Channels.Buffers;

/// <summary>
/// Extension methods for <see cref="IReadableByteBuffer"/> for creating checkpoints.
/// </summary>
public static class ReadableByteBufferCheckpointExtensions
{
    /// <summary>
    /// Creates a checkpoint for the current buffer position for speculative reads.
    /// The checkpoint buffer returned is a view of the original buffer, sharing the same underlying data but with an independent offset.
    /// </summary>
    /// <returns>A readable checkpoint instance</returns>
    public static IReadableCheckpoint Checkpoint( this IReadableByteBuffer source )
        => new ReadableCheckpoint( source );
}
