namespace Faactory.Channels.Buffers;

/// <summary>
/// Provides extension methods for IByteBuffer instances
/// </summary>
public static class ByteBufferExtensions
{
    /// <summary>
    /// Creates a readable buffer from the given source by copying the currently written portion of the given writable buffer
    /// </summary>
    /// <param name="source">The source writable buffer</param>
    /// <param name="endianness">The buffer instance endianness; if null, the source buffer endianness is used.</param>
    /// <returns>A new readable buffer instance with a copy of the currently written portion of the source buffer</returns>
    public static IReadableByteBuffer AsReadable( this IWritableByteBuffer source, Endianness? endianness = null )
        => new ReadableByteBuffer( source.ToArray(), endianness ?? source.Endianness );

    /// <summary>
    /// Creates a writable buffer and copies the entire content of the given readable buffer to it
    /// </summary>
    /// <param name="source">The source readable buffer</param>
    /// <param name="endianness">The buffer instance endianness; if null, the source buffer endianness is used.</param>
    /// <returns>A new writable buffer instance with a copy of the entire content of the source buffer</returns>
    public static IWritableByteBuffer AsWritable( this IReadableByteBuffer source, Endianness? endianness = null )
    {
        var capacity = Math.Max( source.Length * 2, WritableByteBuffer.InitialCapacity );

        var buffer = new WritableByteBuffer( capacity, endianness ?? source.Endianness );

        buffer.WriteBytes( source.ToArray() );

        return buffer;
    }

    /// <summary>
    /// Writes a range of bytes
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <param name="value">The byte[] value</param>
    public static IWritableByteBuffer WriteBytes( this IWritableByteBuffer source, byte[] value )
        => source.WriteBytes( value, 0, value.Length );

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
}
