using Faactory.Channels.Examples;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder( args );

/*
Set up Channels middleware;
We used named channels here to demonstrate how to set up multiple channels
*/
builder.Services.AddChannels()
    .Add( "uppercase", channel =>
    {
        // set up input pipeline
        channel.AddInputAdapter<LetterAdapter>()
            .AddInputHandler<LetterHandler>();
    } )
    .Add( "lowercase", channel =>
    {
        // set up input pipeline
        channel.AddInputAdapter<LetterAdapter>()
            .AddInputAdapter<LowercaseAdapter>()
            .AddInputHandler<LetterHandler>();
    } );

/*
Required for WebSocket channels
*/
builder.Services.AddWebSocketChannels();

/*
Configure Kestrel on port 8080
*/
builder.Services.Configure<KestrelServerOptions>( options =>
{
    options.ListenAnyIP( 8080 );
} );

var app = builder.Build();

// use ASP.NET WebSockets middleware
app.UseWebSockets();

// map WebSockets endpoints to configured Channels middleware
app.MapWebSocketChannel( "/ws/uppercase", "uppercase" );
app.MapWebSocketChannel( "/ws/lowercase", "lowercase" );

app.Run();
