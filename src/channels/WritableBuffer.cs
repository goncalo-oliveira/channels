namespace Faactory.Channels;

internal class WritableBuffer : IWritableBuffer
{
    private readonly List<object> outputData = new();

    public void Write( object data )
    {
        outputData.Add( data );
    }

    internal async Task WriteAsync( IChannel channel )
    {
        if ( !outputData.Any() )
        {
            return;
        }

        foreach ( var data in outputData )
        {
            await channel.WriteAsync( data )
                .ConfigureAwait( false );
        }
    }
}
