namespace Faactory.Channels;

internal class ChannelInfo( IChannel channel ) : IChannelInfo
{
    public string Id => channel.Id;
    public string Name => channel.Name;
    public IReadOnlyDictionary<string, object> Data { get; } = channel.Data.AsReadOnly();
    public DateTimeOffset Created => channel.Created;
    public DateTimeOffset? LastReceived => channel.LastReceived;
    public DateTimeOffset? LastSent => channel.LastSent;
}
