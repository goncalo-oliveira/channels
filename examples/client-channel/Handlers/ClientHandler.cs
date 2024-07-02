using System.Text.RegularExpressions;
using Faactory.Channels.Buffers;
using Faactory.Channels.Handlers;
using Microsoft.Extensions.Logging;

namespace Faactory.Channels.Examples;

/*
The handler receives the response from the server and logs it.
*/

public class ClientHandler( ILoggerFactory loggerFactory ) : ChannelHandler<IByteBuffer>
{
    private readonly ILogger logger = loggerFactory.CreateLogger<ClientHandler>();

    public override Task ExecuteAsync( IChannelContext context, IByteBuffer data )
    {
        var text = System.Text.Encoding.UTF8.GetString( data.ToArray() );

        logger.LogInformation( "Received: {text}", text );

        return Task.CompletedTask;
    }
}
