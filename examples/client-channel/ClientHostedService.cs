using Faactory.Channels.Client;
using Microsoft.Extensions.Hosting;

namespace Faactory.Channels.Examples;

internal sealed class ClientHostedService( IChannelsClientFactory channelsClientFactory, IHostApplicationLifetime hostApplicationLifetime ) : BackgroundService
{
    private readonly IChannelsClientFactory clientFactory = channelsClientFactory;
    private readonly IHostApplicationLifetime appLifetime = hostApplicationLifetime;

    protected override async Task ExecuteAsync( CancellationToken stoppingToken )
    {
        /*
        this service will connect to the server and send a message
        */

        await Task.Delay( 5000, stoppingToken ); // wait for the server to start

        using var client = clientFactory.Create( "client" );

        // wait for the client to connect
        while ( client.Channel.IsClosed )
        {
            await Task.Delay( 1000, stoppingToken );
        }

        await client.Channel.WriteAsync( "Hello, world!" );

        // wait for the server to respond
        await Task.Delay( 5000, stoppingToken );

        // close the client channel
        await client.CloseAsync();

        appLifetime.StopApplication();
    }
}
