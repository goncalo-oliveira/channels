using System.Collections.ObjectModel;

namespace Faactory.Channels;

internal class ChannelInfo( IChannel channel ) : IChannelInfo
{
    public string Id => channel.Id;

    public IReadOnlyDictionary<string, string> Data { get; } = new ReadOnlyDictionary<string, string>( channel.Data );
}
