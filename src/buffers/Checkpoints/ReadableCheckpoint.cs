namespace Faactory.Channels.Buffers;

internal sealed class ReadableCheckpoint( IReadableByteBuffer source ) : IReadableCheckpoint
{
    private readonly int initialOffset = source.Offset;
    private bool isCompleted = false;

    public IReadableByteBuffer Buffer { get; private set; } = source.GetByteBuffer( source.Offset, source.ReadableBytes );

    public void Commit()
    {
        if ( isCompleted )
        {
            throw new InvalidOperationException( "Checkpoint has already completed." );
        }

        source.Seek( initialOffset + Buffer.Offset );

        Dispose();
    }

    public void Dispose()
    {
        if ( isCompleted )
        {
            return;
        }

        // reset the checkpoint buffer to an empty state to avoid accidental usage after completion
        // this also releases any references to the underlying buffer data
        Buffer = ReadableByteBuffer.Empty;

        isCompleted = true;
    }
}
