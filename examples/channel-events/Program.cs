using Faactory.Channels;
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
    We need at least one middleware component to "consume" the data
    or the pipeline buffer will just keep growing
    */
    channel.AddInputHandler<VoidHandler>();

    // set up output pipeline
    /*
    Nothing required
    */
} );

// set up channel monitor
/*
We can add as many monitors as we want.
We're adding this one as singleton but that's not a requirement;
transient and scoped are valid as well, depending on our needs.
*/
builder.Services.AddSingleton<IChannelMonitor, ChannelMonitor>();

// set up TCP channel listener
builder.Services.AddTcpChannelListener( options =>
{
    options.Port = 8080;
    options.Backlog = 10;
} );

var app = builder.Build();

app.Run();
