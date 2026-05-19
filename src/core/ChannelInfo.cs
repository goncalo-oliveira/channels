namespace Faactory.Channels;

internal class ChannelInfo( IChannel channel ) : IChannelInfo
{
    public string Id => channel.Id;

    public IReadOnlyDictionary<string, object> Data { get; } = channel.Data.AsReadOnly();
}
