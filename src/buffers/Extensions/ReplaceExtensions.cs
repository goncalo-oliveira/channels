namespace Faactory.Channels.Buffers;

/// <summary>
/// Provides extension methods for replacing byte sequences in an <see cref="IWritableByteBuffer"/>.
/// </summary>
public static class ByteBufferReplaceExtensions
{
    /// <summary>
    /// Replaces all occurrences of a sequence of bytes with another
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <param name="sequence">The sequence of bytes to replace</param>
    /// <param name="replacement">The sequence of bytes to replace with</param>
    /// <returns>The same writable buffer.</returns>
    public static IWritableByteBuffer ReplaceBytes( this IWritableByteBuffer source, byte[] sequence, byte[] replacement )
    {
        if ( sequence.Length == 0 )
        {
            return source;
        }

        var span = source.AsSpan();
        var result = new WritableByteBuffer( span.Length, source.Endianness );

        int index = 0;

        while ( true )
        {
            int match = span[index..].IndexOf( sequence );

            if ( match < 0 )
            {
                break;
            }

            match += index;

            result.WriteBytes( span[index..match] );
            result.WriteBytes( replacement );

            index = match + sequence.Length;
        }

        result.WriteBytes( span[index..] );

        source.Truncate();
        source.WriteBytes( result.AsSpan() );

        return source;
    }
}
