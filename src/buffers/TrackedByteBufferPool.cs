namespace Faactory.Channels.Buffers;

/// <summary>
/// Implements a tracked byte buffer pool that wraps the <see cref="ByteBufferPool"/> and keeps track of rented buffers, allowing for automatic disposal of all rented buffers when the pool is disposed.
/// </summary>
public sealed class TrackedByteBufferPool : IByteBufferPool, IDisposable
{
    private readonly ByteBufferPool pool = new();
    private readonly List<IWritableByteBuffer> rented = [];
    private bool disposed;

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
    {
        ObjectDisposedException.ThrowIf( disposed, this );

        IWritableByteBuffer? buffer = null;

        buffer = new WritableByteBuffer(
            capacity,
            allocator: pool.Allocate,
            releaser: bytes =>
            {
                pool.Release( bytes );
                rented.Remove( buffer! );
            }
        );

        rented.Add( buffer );

        return buffer;
    }

    /// <summary>
    /// Disposes the pool and all rented buffers, returning their underlying byte arrays to the pool for reuse.
    /// </summary>
    public void Dispose()
    {
        if ( disposed )
        {
            return;
        }

        disposed = true;

        foreach ( var buffer in rented.ToArray() )
        {
            buffer.Dispose();
        }

        rented.Clear();
    }
}
