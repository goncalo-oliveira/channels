using Faactory.Channels.Examples;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder( args );

builder.Services.AddChannels( channel =>
{
    // configure options
    channel.Configure( options =>
    {
        options.Port = 8080;
        options.Backlog = 10;
    } );

    // set up long-running services
    // since v0.5 idle monitoring is a channel service
    channel.AddIdleChannelService();

    // set up input pipeline
    channel.AddInputAdapter<WordAdapter>()
        .AddInputHandler<WordHandler>();

    // set up output pipeline
    channel.AddOutputAdapter<UTFEncoderAdapter>();
} );

var app = builder.Build();

app.Run();
