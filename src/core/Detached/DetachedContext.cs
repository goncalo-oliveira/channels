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

    /// <summary>
    /// The detached channel, which is a no-op channel.
    /// </summary>
    public IChannel Channel { get; } = new DetachedChannel();

    /// <summary>
    /// The endianness of the buffer, which is big-endian by default.
    /// </summary>
    public Buffers.Endianness BufferEndianness { get; internal set; } = Buffers.Endianness.BigEndian;

    /// <summary>
    /// The output buffer.
    /// </summary>
    public IWritableBuffer Output => this;

    /// <summary>
    /// The forwarded data log.
    /// </summary>
    public object[] Forwarded => forwarded.ToArray();

    /// <summary>
    /// The written data log.
    /// </summary>
    public object[] Written => written.ToArray();

    /// <summary>
    /// The custom events log.
    /// </summary>
    public KeyValuePair<string, object?>[] CustomEvents => customEvents.ToArray();

    /// <summary>
    /// Forwards data to the detached channel, which simply logs it for testing purposes.
    /// </summary>
    public void Forward( object data )
        => forwarded.Add( data );

    /// <summary>
    /// Clears the forwarded data log.
    /// </summary>
    public void Clear()
        => forwarded.Clear();

    /// <summary>
    /// Forwards multiple data items to the detached channel, which simply logs them for testing purposes.
    /// </summary>
    public void ForwardMany( object[] collection )
        => forwarded.AddRange( collection );

    /// <summary>
    /// Notifies a custom event, which simply logs it for testing purposes.
    /// </summary>
    public void NotifyCustomEvent( string name, object? data = null )
        => customEvents.Add( new( name, data ) );

    /// <summary>
    /// Writes data to the detached channel, which simply logs it for testing purposes.
    /// </summary>
    public void Write( object data )
        => written.Add( data );
}
