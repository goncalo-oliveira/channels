using System.Collections.ObjectModel;

namespace Faactory.Channels;

internal class ChannelInfo( IChannel channelInstance ) : IChannelInfo
{
    private readonly IChannel channel = channelInstance;

    public string Id => channel.Id;

    public IReadOnlyDictionary<string, string> Data => new ReadOnlyDictionary<string, string>( channel.Data );
}
