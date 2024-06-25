using Faactory.Channels.Examples;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder( args );

builder.Services.AddChannels( channel =>
{
    // set up long-running services
    channel.AddIdleChannelService();

    // set up input pipeline
    /*
    We are replying the received data as it is, therefore we don't need adapters
    */
    channel.AddInputHandler<EchoHandler>();

    // set up output pipeline
    /*
    We are replying the received data as it is, therefore we don't need adapters
    */
} );

// set up TCP channel listener
builder.Services.AddTcpChannelListener( options =>
{
    options.Port = 8080;
    options.Backlog = 10;
} );

var app = builder.Build();

app.Run();
