namespace Faactory.Channels.Buffers;

/// <summary>
/// Provides extension methods for IWritableByteBuffer instances
/// </summary>
public static class WritableByteBufferExtensions
{
    /// <summary>
    /// Creates a writable view of the buffer starting at the specified offset and extending to the end of the used portion of the buffer.
    /// The returned view shares the same underlying memory, allowing for zero-copy modifications.
    /// The offset must be within the bounds of the buffer's capacity.
    /// Modifying the returned view will affect the original buffer, and vice versa.
    /// The returned view is limited to the portion of the buffer starting from the specified offset to the end of the used portion of the buffer.
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <param name="offset">The starting offset of the view</param>
    /// <returns>A writable view of the buffer</returns>
    public static IWritableByteBufferView CreateView( this IWritableByteBuffer source, int offset )
        => source.CreateView( offset, source.Length - offset );

    /// <summary>
    /// Writes a range of bytes
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <param name="value">The byte[] value</param>
    public static IWritableByteBuffer WriteBytes( this IWritableByteBuffer source, byte[] value )
        => source.WriteBytes( value, 0, value.Length );
}
