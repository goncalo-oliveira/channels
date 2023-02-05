using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder( args );

builder.Services.AddChannelsHostedService( channels =>
{
    // configure options
    channels.Configure( options =>
    {
        options.Port = 8080;
        options.Backlog = 10;
    } );

    // set up long-running services
    // since v0.5 idle monitoring is a channel service
    channels.AddIdleChannelService();

    // set up input pipeline
    /*
    We are replying the received data as it is, therefore we don't need adapters
    */
    channels.AddInputHandler<EchoHandler>();

    // set up output pipeline
    /*
    We are replying the received data as it is, therefore we don't need adapters
    */
} );

var app = builder.Build();

await app.RunAsync();
