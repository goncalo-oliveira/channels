using Faactory.Channels;
using Faactory.Channels.Adapters;

/// <summary>
/// An adapter context to be used outside a Channels context
/// </summary>
public sealed class DetachedContext : IAdapterContext
{
    private readonly List<object> forwarded = new List<object>();

    public IChannel Channel { get; } = new DetachedChannel();

    public IWritableBuffer Output { get; } = new WritableBuffer();

    public IEnumerable<object> Forwarded => forwarded.ToArray();

    public void Forward( object data )
        => forwarded.Add( data );

    public void Clear()
        => forwarded.Clear();

    public void ForwardMany( object[] collection )
        => forwarded.AddRange( collection );

    public void NotifyCustomEvent( string name, object? data = null )
    {
        throw new NotImplementedException();
    }

    private class WritableBuffer : IWritableBuffer
    {
        public void Write( object data )
        {
            // do nothing...
        }
    }
}
