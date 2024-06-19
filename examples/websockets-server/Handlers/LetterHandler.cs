using Faactory.Channels.Handlers;

namespace Faactory.Channels.Examples;

/*
The handler receives the words in the phrase and replies back.
This is where we'd do operational tasks, such as logging the words for example
or executing a command.
*/

public class LetterHandler : ChannelHandler<char[]>
{
    public override Task ExecuteAsync( IChannelContext context, char[] letters )
    {
        /*
        we want to respond with a string that contains all the letters that were received

        Take note that we are sending a string object.
        The output pipeline will send this as a Text message.
        */
        context.Output.Write( string.Concat( letters ) );

        return Task.CompletedTask;
    }
}
