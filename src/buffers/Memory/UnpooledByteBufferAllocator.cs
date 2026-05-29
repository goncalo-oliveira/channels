namespace Faactory.Channels.Buffers.Memory;

/// <summary>
/// Implements a default byte buffer allocator that creates new byte arrays for each allocation.
/// This allocator performs direct allocations and does not pool buffers.
/// </summary>
public sealed class UnpooledByteBufferAllocator : IByteBufferAllocator
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="UnpooledByteBufferAllocator"/>.
    /// This instance can be used wherever a default byte buffer allocator is needed without the overhead of pooling.
    /// </summary>
    public static UnpooledByteBufferAllocator Instance { get; } = new();

    /// <summary>
    /// Allocates a byte array with the specified size.
    /// </summary>
    /// <param name="size">The size of the byte array to allocate</param>
    /// <returns>A byte array with the specified size</returns>
    public byte[] Allocate( int size )
    {
        return new byte[size];
    }

    /// <summary>
    /// Releases a previously allocated byte array. This is a no-op for the default allocator.
    /// </summary>
    /// <param name="buffer">The byte array to release</param>
    public void Release( byte[] buffer )
    {
        // No-op for default allocator
    }
}
