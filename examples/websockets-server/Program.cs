using Faactory.Channels.Examples;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder( args );

builder.Logging.ClearProviders()
    .AddSimpleConsole( options =>
    {
        options.IncludeScopes = false;
        options.SingleLine = true;
    } );

/*
Set up Channels over WebSockets
*/
builder.Services.AddWebSocketChannels( channel =>
{
    // set up long-running services
    // since v0.5 idle monitoring is a channel service
    channel.AddIdleChannelService();

    // set up input pipeline
    channel.AddInputAdapter<LetterAdapter>()
        .AddInputHandler<LetterHandler>();
} );

/*
Configure Kestrel on port 8080
*/
builder.Services.Configure<KestrelServerOptions>( options =>
{
    options.ListenAnyIP( 8080 );
} );

var app = builder.Build();

// set up the WebSocket middleware
app.UseWebSockets();

// map WebSocket endpoint to Channels middleware
app.MapWebSocketChannel( "/ws/connect" );

app.Run();
