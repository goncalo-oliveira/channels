namespace Faactory.Channels.Buffers;

/// <summary>
/// Exception thrown when a write operation is attempted over a non-writable <see cref="IByteBuffer"/>.
/// </summary>
public sealed class NonWritableBufferException : InvalidOperationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NonWritableBufferException"/> class with a default error message.
    /// </summary>
    public NonWritableBufferException()
        : base( "Invalid operation over a non-writable IByteBuffer." )
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NonWritableBufferException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
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
