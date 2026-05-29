namespace Faactory.Channels.Buffers;

/// <summary>
/// Provides extension methods for IByteBuffer instances
/// </summary>
public static class ByteBufferExtensions
{
    /// <summary>
    /// Converts the buffer to a base64 encoded string
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <returns>The base64 encoded string</returns>
    public static string ToBase64String( this IByteBuffer source )
        => Convert.ToBase64String( source.AsSpan() );

    /// <summary>
    /// Converts the buffer to a hexadecimal string
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <returns>The hexadecimal string</returns>
    public static string ToHexString( this IByteBuffer source )
        => Convert.ToHexString( source.AsSpan() );

    /// <summary>
    /// Copies the contents of the buffer to the destination span
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <param name="destination">The destination span</param>
    public static void CopyTo( this IByteBuffer source, Span<byte> destination )
    {
        source.AsSpan().CopyTo( destination );
    }

    /// <summary>
    /// Copies a portion of the buffer to the destination span
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <param name="destination">The destination span</param>
    /// <param name="offset">The offset in the source buffer</param>
    /// <param name="length">The number of bytes to copy</param>
    public static void CopyTo( this IByteBuffer source, Span<byte> destination, int offset, int length )
    {
        ArgumentOutOfRangeException.ThrowIfNegative( offset, nameof( offset ) );
        ArgumentOutOfRangeException.ThrowIfNegative( length, nameof( length ) );

        source.AsSpan()
            .Slice(offset, length)
            .CopyTo(destination);
    }
}
