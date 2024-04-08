namespace Faactory.Channels;

/// <summary>
/// A data holder available throughout the entire channel session
/// </summary>
public sealed class ChannelData : Dictionary<string, string>
{
    public ChannelData()
        : base( StringComparer.OrdinalIgnoreCase )
    { }

    public ChannelData( int capacity )
        : base( capacity, StringComparer.OrdinalIgnoreCase )
    { }

    public ChannelData( IDictionary<string, string> dictionary )
        : base( dictionary, StringComparer.OrdinalIgnoreCase )
    { }

    public ChannelData( IEnumerable<KeyValuePair<string, string>> collection )
        : base( collection, StringComparer.OrdinalIgnoreCase )
    { }
}
