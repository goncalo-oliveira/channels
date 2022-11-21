namespace Faactory.Channels.Buffers;

public static class ByteBufferSearchExtensions
{
    /// <summary>
    /// Finds a sequence of bytes
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <param name="sequence">The sequence of bytes to find</param>
    /// <param name="offset">The offset in the buffer to start looking; if -1 it uses the buffer's current offset</param>
    /// <returns>The index of the beginning of the sequence; -1 if the sequence wasn't found.</returns>
    public static int FindBytes( this IByteBuffer source, byte[] sequence, int offset = -1 )
    {
        if ( !source.IsReadable )
        {
            throw new InvalidOperationException( "Invalid operation over a non-readable IByteBuffer." );
        }

        if ( offset < 0 )
        {
            offset = source.Offset;
        }

        // look for a sequence match
        for ( int idx = offset; idx < source.ReadableBytes; idx++ )
        {
            if ( MatchesSequence( source, sequence, idx ) )
            {
                return ( idx );
            }
        }

        return ( - 1 );
    }

    /// <summary>
    /// Matches a sequence of bytes at a given position
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <param name="sequence">The sequence of bytes to match</param>
    /// <param name="offset">The offset in the buffer; if -1 it uses the buffer's current offset</param>
    /// <returns>True if the sequence matches; false otherwise.</returns>
    public static bool MatchesSequence( this IByteBuffer source, byte[] sequence, int offset = -1 )
    {
        if ( !source.IsReadable )
        {
            throw new InvalidOperationException( "Invalid operation over a non-readable IByteBuffer." );
        }

        if ( offset < 0 )
        {
            offset = source.Offset;
        }

        if ( ( offset + sequence.Length ) > source.ReadableBytes )
        {
            return ( false );
        }

        for ( int idx = offset; idx < ( offset + sequence.Length ); idx++ )
        {
            if ( source.GetByte( idx ) != sequence[idx - offset] )
            {
                return ( false );
            }
        }

        return ( true );
    }

}
