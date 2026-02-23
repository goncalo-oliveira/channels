namespace Faactory.Channels;

/// <summary>
/// A data holder available throughout the entire channel session
/// </summary>
public sealed class ChannelData : Dictionary<string, string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChannelData"/> class.
    /// </summary>
    public ChannelData()
        : base( StringComparer.OrdinalIgnoreCase )
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChannelData"/> class with the specified initial capacity.
    /// </summary>
    /// <param name="capacity">The initial number of elements that the <see cref="ChannelData"/> can contain.</param>
    public ChannelData( int capacity )
        : base( capacity, StringComparer.OrdinalIgnoreCase )
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChannelData"/> class that contains elements copied from the specified <see cref="IDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <param name="dictionary">The <see cref="IDictionary{TKey, TValue}"/> whose elements are copied to the new <see cref="ChannelData"/>.</param>
    public ChannelData( IDictionary<string, string> dictionary )
        : base( dictionary, StringComparer.OrdinalIgnoreCase )
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChannelData"/> class that contains elements copied from the specified <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <param name="collection">The <see cref="IEnumerable{T}"/> whose elements are copied to the new <see cref="ChannelData"/>.</param>
    public ChannelData( IEnumerable<KeyValuePair<string, string>> collection )
        : base( collection, StringComparer.OrdinalIgnoreCase )
    { }
}
