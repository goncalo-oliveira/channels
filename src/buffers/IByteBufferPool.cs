namespace Faactory.Channels.Buffers;

/// <summary>
/// Defines a pool of byte buffers that can be rented and returned to optimize memory usage and reduce allocations.
/// </summary>
public interface IByteBufferPool
{
    /// <summary>
    /// Rents a byte buffer from the pool with at least the specified capacity.
    /// The returned <see cref="IWritableByteBuffer"/> manages the rented byte array
    /// and will return it to the pool when disposed.
    /// </summary>
    /// <remarks>
    /// The returned <see cref="IWritableByteBuffer"/> must be disposed when no longer needed
    /// so that the underlying byte array can be returned to the pool for reuse.
    /// </remarks>
    /// <param name="capacity">The minimum capacity of the buffer to rent.</param>
    /// <returns>An <see cref="IWritableByteBuffer"/> with at least the specified capacity.</returns>
    IWritableByteBuffer Rent( int capacity );
}
