using System.Text.RegularExpressions;
using Faactory.Channels.Handlers;

namespace Faactory.Channels.Examples;

/*
The handler receives the words in the phrase and replies back.
This is where we'd do operational tasks, such as logging the words for example
or executing a command.
*/

public class WordHandler : ChannelHandler<MatchCollection>
{
    public override Task ExecuteAsync( IChannelContext context, MatchCollection data )
    {
        var response = $"received {data.Count} word(s).\n";

        // Take note here that we are sending a string object
        // This will delivered to the output pipeline
        context.Output.Write( response );

        return Task.CompletedTask;
    }
}
