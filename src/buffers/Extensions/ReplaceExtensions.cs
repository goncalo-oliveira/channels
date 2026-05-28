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

        int index = 0;

        if ( sequence.Length == replacement.Length )
        {
            var span = source.AsSpan();

            while ( true )
            {
                int match = span[index..].IndexOf( sequence );

                if ( match < 0 )
                {
                    break;
                }

                match += index;

                source.CreateView( match ).WriteBytes( replacement );

                index = match + sequence.Length;
            }

            return source;
        }

        var original = source.ToArray();

        source.Truncate();

        while ( true )
        {
            int match = original.AsSpan( index ).IndexOf( sequence );

            if ( match < 0 )
            {
                break;
            }

            match += index;

            source.WriteBytes( original, index, match - index );
            source.WriteBytes( replacement );

            index = match + sequence.Length;
        }

        source.WriteBytes( original, index, original.Length - index );

        return source;
    }
}
