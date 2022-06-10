namespace Faactory.Channels.Buffers;

public static class ByteBufferExtensions
{
    /// <summary>
    /// Creates a read-only buffer from the given source
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <returns><A read-only buffer instance/returns>
    public static IByteBuffer MakeReadOnly( this IByteBuffer source )
    {
        if ( source.IsReadable && !source.IsWritable )
        {
            // already read-only
            return ( source );
        }

        return new WrappedByteBuffer( source.ToArray(), source.Endianness );
    }

    /// <summary>
    /// Creates a writable buffer from the given source
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <returns>A writable buffer instance</returns>
    public static IByteBuffer MakeWritable( this IByteBuffer source )
    {
        if ( source.IsWritable )
        {
            return ( source );
        }

        return new WritableByteBuffer( source.ToArray(), source.Endianness );
    }

    /// <summary>
    /// Writes a range of bytes
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <param name="value">The byte[] value</param>
    public static IByteBuffer WriteBytes( this IByteBuffer source, byte[] value )
        => source.WriteBytes( value, 0, value.Length );
}
