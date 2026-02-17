namespace Faactory.Channels.Buffers;

/// <summary>
/// Exception thrown when a read operation is attempted over a non-readable <see cref="IByteBuffer"/>.
/// </summary>
public sealed class NonReadableBufferException : InvalidOperationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NonReadableBufferException"/> class with a default error message.
    /// </summary>
    public NonReadableBufferException()
        : base( "Invalid operation over a non-readable IByteBuffer." )
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NonReadableBufferException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public NonReadableBufferException( string message )
        : base( message )
    {
    }

    /// <summary>
    /// Throws a <see cref="NonReadableBufferException"/> if the buffer is not readable.
    /// </summary>
    public static void ThrowIfNotReadable( IByteBuffer buffer )
    {
        if ( !buffer.IsReadable )
        {
            throw new NonReadableBufferException();
        }
    }
}
