using Faactory.Channels.Adapters;

namespace Faactory.Channels;

/// <summary>
/// An adapter context to be used outside a Channels context
/// </summary>
public sealed class DetachedContext : IAdapterContext, IWritableBuffer
{
    private readonly List<object> forwarded = [];
    private readonly List<object> written = [];
    private readonly List<KeyValuePair<string, object?>> customEvents = [];

    public IChannel Channel { get; } = new DetachedChannel();

    public IWritableBuffer Output => this;

    public object[] Forwarded => forwarded.ToArray();

    public object[] Written => written.ToArray();

    public KeyValuePair<string, object?>[] CustomEvents => customEvents.ToArray();

    public void Forward( object data )
        => forwarded.Add( data );

    public void Clear()
        => forwarded.Clear();

    public void ForwardMany( object[] collection )
        => forwarded.AddRange( collection );

    public void NotifyCustomEvent( string name, object? data = null )
        => customEvents.Add( new( name, data ) );

    public void Write( object data )
        => written.Add( data );
}
