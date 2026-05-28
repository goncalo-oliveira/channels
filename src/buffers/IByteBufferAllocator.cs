namespace Faactory.Channels.Buffers;

/// <summary>
/// Defines an interface for allocating and releasing byte buffers.
/// </summary>
public interface IByteBufferAllocator
{
    /// <summary>
    /// Allocates a byte array with at least the specified size.
    /// </summary>
    /// <param name="size">The minimum size of the byte array to allocate.</param>
    /// <returns>A byte array with at least the specified size.</returns>
    byte[] Allocate( int size );

    /// <summary>
    /// Releases a byte array back to the allocator for reuse.
    /// </summary>
    /// <param name="buffer">The byte array to release.</param>
    void Release( byte[] buffer );
}
