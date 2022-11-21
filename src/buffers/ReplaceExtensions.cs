namespace Faactory.Channels.Buffers;

public static class ByteBufferReplaceExtensions
{
    /// <summary>
    /// Replaces all occurrences of a sequence of bytes with another
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <param name="sequence">The sequence of bytes to replace</param>
    /// <param name="replacement">The sequence of bytes to replace with</param>
    /// <returns>The same writable buffer.</returns>
    public static IByteBuffer ReplaceBytes( this IByteBuffer source, byte[] sequence, byte[] replacement )
    {
        if ( !source.IsWritable )
        {
            throw new InvalidOperationException( "Invalid operation over a non-writable IByteBuffer." );
        }

        var readableSource = source.MakeReadOnly();
        var escapeIndex = -1;
        do
        {
            escapeIndex = readableSource.FindBytes( sequence, escapeIndex );

            if ( escapeIndex > -1 )
            {
                // replace it!
                source.DiscardAll();

                source.WriteBytes( readableSource.GetBytes( 0, escapeIndex ) );
                source.WriteBytes( replacement );
                source.WriteBytes( readableSource.GetBytes( escapeIndex + sequence.Length, readableSource.ReadableBytes - ( escapeIndex + sequence.Length ) ) );

                readableSource = source.MakeReadOnly();
                escapeIndex += replacement.Length;
            }

        } while ( escapeIndex > -1 );

        return ( source );
    }
}
