using System.Buffers;

namespace Faactory.Channels.Buffers;

/// <summary>
/// Implements a byte buffer pool using the shared ArrayPool{byte} to rent and return byte arrays, optimizing memory usage and reducing allocations.
/// </summary>
public sealed class ByteBufferPool : IByteBufferPool
{
    private readonly ArrayPool<byte> pool = ArrayPool<byte>.Shared;

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
    /// <returns>An <see cref="IWritableByteBuffer"/> with the specified capacity.</returns>
    public IWritableByteBuffer Rent( int capacity )
        => new WritableByteBuffer(
            allocator: size => pool.Rent( size ),
            releaser: buffer => pool.Return( buffer )
        );
}

/// <summary>
/// Provides extension methods for the IByteBufferPool interface
/// </summary>
public static class ByteBufferPoolExtensions
{
    /// <summary>
    /// Rents a byte buffer from the pool with the default initial capacity.
    /// The returned <see cref="IWritableByteBuffer"/> manages the rented byte array
    /// and will return it to the pool when disposed.
    /// </summary>
    /// <remarks>
    /// The returned <see cref="IWritableByteBuffer"/> must be disposed when no longer needed
    /// so that the underlying byte array can be returned to the pool for reuse.
    /// </remarks>
    /// <param name="pool">The byte buffer pool to rent from.</param>
    /// <returns>An <see cref="IWritableByteBuffer"/> with the default initial capacity.</returns>
    public static IWritableByteBuffer Rent( this IByteBufferPool pool )
    {
        ArgumentNullException.ThrowIfNull( pool );

        return pool.Rent( WritableByteBuffer.InitialCapacity );
    }
}
