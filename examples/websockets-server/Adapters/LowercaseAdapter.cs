using Faactory.Channels.Adapters;
using Faactory.Channels.WebSockets;

namespace Faactory.Channels.Examples;

/*
This adapters reads a sequence of characters and converts them to lowercase.

Take note that this adapter is an `IInputChannelAdapter`.
*/

public partial class LowercaseAdapter : ChannelAdapter<char[]>, IInputChannelAdapter
{
    public override Task ExecuteAsync( IAdapterContext context, char[] letters )
    {
        var lowercase = letters
            .Select( c => char.ToLowerInvariant( c ) )
            .ToArray();

        context.Forward( lowercase );

        return Task.CompletedTask;
    }
}
