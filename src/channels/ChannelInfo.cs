using System.Collections.ObjectModel;

namespace Faactory.Channels;

internal class ChannelInfo : IChannelInfo
{
    private readonly IChannel channel;
    public ChannelInfo( IChannel channelInstance )
    {
        channel = channelInstance;
    }

    public string Id => channel.Id;

    public IReadOnlyDictionary<string, string> Data => new ReadOnlyDictionary<string, string>( channel.Data );
}
