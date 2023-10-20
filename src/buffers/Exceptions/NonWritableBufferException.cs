namespace Faactory.Channels.Buffers;

/// <summary>
/// Exception thrown when a write operation is attempted over a non-writable <see cref="IByteBuffer"/>.
/// </summary>
public sealed class NonWritableBufferException : InvalidOperationException
{
    public NonWritableBufferException()
        : base( "Invalid operation over a non-writable IByteBuffer." )
    {
    }

    public NonWritableBufferException( string message )
        : base( message )
    {
    }

    /// <summary>
    /// Throws a <see cref="NonWritableBufferException"/> if the buffer is not writable.
    /// </summary>
    public static void ThrowIfNotWritable( IByteBuffer buffer )
    {
        if ( !buffer.IsWritable )
        {
            throw new NonWritableBufferException();
        }
    }
}
