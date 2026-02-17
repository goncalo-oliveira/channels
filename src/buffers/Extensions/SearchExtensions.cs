namespace Faactory.Channels.Buffers;

/// <summary>
/// Provides extension methods for searching within byte buffers.
/// </summary>
public static class ByteBufferSearchExtensions
{
    /// <summary>
    /// Determines whether any byte in the buffer satisfies the condition.
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <param name="predicate">A function to test each byte for a condition.</param>
    /// <param name="offset">The offset in the buffer to start looking; if -1 it uses the buffer's current offset</param>
    /// <returns>True if any byte satisfies the condition; false otherwise.</returns>
    public static bool Any( this IReadableByteBuffer source, Func<byte, bool> predicate, int offset = -1 )
    {
        offset = offset < 0
            ? source.Offset
            : offset;

        if ( offset >= source.Length )
        {
            return false;
        }

        var span = source.GetSpan( offset, source.Length - offset );

        foreach ( var b in span )
        {
            if ( predicate( b ) )
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Finds a byte
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <param name="b">The byte to find</param>
    /// <param name="offset">The offset in the buffer to start looking; if -1 it uses the buffer's current offset</param>
    /// <returns>The index of the byte; -1 if the byte wasn't found.</returns>
    public static int IndexOf( this IReadableByteBuffer source, byte b, int offset = -1 )
    {
        offset = offset < 0
            ? source.Offset
            : offset;

        if ( offset >= source.Length )
        {
            return -1;
        }

        var span = source.GetSpan( offset, source.Length - offset );
        int idx = span.IndexOf( b );

        return idx >= 0
            ? offset + idx
            : -1;
    }

    /// <summary>
    /// Finds a sequence of bytes
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <param name="sequence">The sequence of bytes to find</param>
    /// <param name="offset">The offset in the buffer to start looking; if -1 it uses the buffer's current offset</param>
    /// <returns>The index of the beginning of the sequence; -1 if the sequence wasn't found.</returns>
    public static int IndexOf( this IReadableByteBuffer source, byte[] sequence, int offset = -1 )
    {
        if ( sequence.Length == 0 )
        {
            return -1;
        }

        offset = offset < 0
            ? source.Offset
            : offset;

        if ( offset > source.Length - sequence.Length )
        {
            return -1;
        }

        var span = source.GetSpan( offset, source.Length - offset );
        int idx = span.IndexOf( sequence );

        return idx >= 0
            ? offset + idx
            : -1;
    }

    /// <summary>
    /// Matches a sequence of bytes at a given position
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <param name="sequence">The sequence of bytes to match</param>
    /// <param name="offset">The offset in the buffer; if -1 it uses the buffer's current offset</param>
    /// <returns>True if the sequence matches; false otherwise.</returns>
    public static bool MatchBytes( this IReadableByteBuffer source, byte[] sequence, int offset = -1 )
    {
        if ( sequence.Length == 0 )
        {
            return false;
        }

        offset = offset < 0 ? source.Offset : offset;

        if ( offset > source.Length - sequence.Length )
        {
            return false;
        }

        return source.GetSpan( offset, sequence.Length )
            .SequenceEqual( sequence );
    }

}
