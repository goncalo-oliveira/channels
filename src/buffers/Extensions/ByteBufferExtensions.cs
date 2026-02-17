namespace Faactory.Channels.Buffers;

/// <summary>
/// Provides extension methods for IByteBuffer instances
/// </summary>
public static class ByteBufferExtensions
{
    /// <summary>
    /// Creates a readable buffer from the given source
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <param name="endianness">The buffer instance endianness; if null, the source buffer endianness is used.</param>
    /// <returns>A new readable buffer instance</returns>
    public static IReadableByteBuffer AsReadable( this IWritableByteBuffer source, Endianness? endianness = null )
        => new ReadableByteBuffer( source.ToArray(), endianness ?? source.Endianness );

    /// <summary>
    /// Ensures that the given buffer is readable, creating a new readable buffer only if necessary.
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <param name="endianness">The buffer instance endianness; if null, the source buffer endianness is used.</param>
    /// <returns>The original buffer if it is already readable and has the correct endianness, or a new readable buffer instance otherwise</returns>
    public static IReadableByteBuffer EnsureReadable( this IByteBuffer source, Endianness? endianness = null )
    {
        // if the source buffer is already readable and the endianness matches (or is not specified),
        // we can return it directly without creating a new wrapper.
        if ( source is IReadableByteBuffer readable && ( endianness is null || endianness == readable.Endianness ) )
        {
            return readable;
        }

        return new ReadableByteBuffer( source.ToArray(), endianness ?? source.Endianness );
    }

    /// <summary>
    /// Creates a writable buffer from the given source
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <param name="endianness">The buffer instance endianness; if null, the source buffer endianness is used.</param>
    /// <returns>A new writable buffer instance</returns>
    public static IWritableByteBuffer AsWritable( this IReadableByteBuffer source, Endianness? endianness = null )
    {
        var capacity = Math.Max( source.Length * 2, WritableByteBuffer.InitialCapacity );

        var buffer = new WritableByteBuffer( capacity, endianness ?? source.Endianness );

        buffer.WriteBytes( source.ToArray() );

        return buffer;
    }

    /// <summary>
    /// Ensures that the given buffer is writable, creating a new writable buffer only if necessary.
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <param name="endianness">The buffer instance endianness; if null, the source buffer endianness is used.</param>
    /// <returns>The original buffer if it is already writable and has the correct endianness, or a new writable buffer instance otherwise</returns>
    public static IWritableByteBuffer EnsureWritable( this IByteBuffer source, Endianness? endianness = null )
    {
        // if the source buffer is already writable and the endianness matches (or is not specified),
        // we can return it directly without creating a new wrapper.
        if ( source is IWritableByteBuffer writable && ( endianness is null || endianness == writable.Endianness ) )
        {
            return writable;
        }

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
