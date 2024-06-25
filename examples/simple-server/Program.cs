using Faactory.Channels.Examples;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder( args );

builder.Services.AddChannels( channel =>
{
    // set up long-running services
    channel.AddIdleChannelService();

    // set up input pipeline
    channel.AddInputAdapter<WordAdapter>()
        .AddInputHandler<WordHandler>();

    // set up output pipeline
    channel.AddOutputAdapter<UTFEncoderAdapter>();
} );

// set up TCP channel listener
builder.Services.AddTcpChannelListener( options =>
{
    options.Port = 8080;
    options.Backlog = 10;
} );

var app = builder.Build();

app.Run();
