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
    channels.AddInputAdapter<WordAdapter>()
        .AddInputHandler<WordHandler>();

    // set up output pipeline
    channels.AddOutputAdapter<UTFEncoderAdapter>();
} );

var app = builder.Build();

await app.RunAsync();
