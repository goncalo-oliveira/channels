using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
    We need one at least one middleware component to "consume" the data
    or the pipeline buffer will just keep growing
    */
    channels.AddInputHandler<VoidHandler>();

    // set up output pipeline
    /*
    Nothing required
    */

    // set up channel event listeners
    /*
    We can add as many listeners as we want.
    We're adding this one as singleton but that's not a requirement;
    transient and scoped are valid as well, depending on our needs.
    */
    channels.Services.AddSingleton<Faactory.Channels.IChannelEvents, EventListener>();
} );

var app = builder.Build();

await app.RunAsync();
