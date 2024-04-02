namespace Faactory.Channels.Buffers;

public static class ByteBufferExtensions
{
    /// <summary>
    /// Creates a read-only buffer from the given source
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <param name="endianness">The buffer instance endianness; if null, the source buffer endianness is used.</param>
    /// <returns>A read-only buffer instance</returns>
    public static IByteBuffer MakeReadOnly( this IByteBuffer source, Endianness? endianness = null )
    {
        if ( source.IsReadable && !source.IsWritable )
        {
            /*
            the source buffer is already read-only.
            we might still need to change the endianness, though.
            */
            if ( ( endianness != null ) && endianness != source.Endianness )
            {
                return new WrappedByteBuffer( source.ToArray(), endianness.Value );
            }

            // no need to create a new buffer wrapper
            return ( source );
        }

        return new WrappedByteBuffer( source.ToArray(), endianness ?? source.Endianness );
    }

    /// <summary>
    /// Creates a writable buffer from the given source
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <param name="endianness">The buffer instance endianness; if null, the source buffer endianness is used.</param>
    /// <returns>A writable buffer instance</returns>
    public static IByteBuffer MakeWritable( this IByteBuffer source, Endianness? endianness = null )
    {
        if ( source.IsWritable )
        {
            /*
            the source buffer is already writable.
            we might still need to change the endianness, though.
            */
            if ( ( endianness != null ) && endianness != source.Endianness )
            {
                return new WritableByteBuffer( source.ToArray(), endianness.Value );
            }

            return ( source );
        }

        return new WritableByteBuffer( source.ToArray(), endianness ?? source.Endianness );
    }

    /// <summary>
    /// Writes a range of bytes
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <param name="value">The byte[] value</param>
    public static IByteBuffer WriteBytes( this IByteBuffer source, byte[] value )
        => source.WriteBytes( value, 0, value.Length );

    /// <summary>
    /// Converts the buffer to a base64 encoded string
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <returns>The base64 encoded string</returns>
    public static string ToBase64String( this IByteBuffer source )
        => Convert.ToBase64String( source.ToArray() );

    /// <summary>
    /// Converts the buffer to a hexadecimal string
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <returns>The hexadecimal string</returns>
    public static string ToHexString( this IByteBuffer source )
        => string.Concat( source.ToArray().Select( b => string.Format( "{0:X2}", b ) ) );
}
