namespace Faactory.Channels.Buffers;

/// <summary>
/// Exception thrown when a read operation is attempted over a non-readable <see cref="IByteBuffer"/>.
/// </summary>
public sealed class NonReadableBufferException : InvalidOperationException
{
    public NonReadableBufferException()
        : base( "Invalid operation over a non-readable IByteBuffer." )
    {
    }

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
