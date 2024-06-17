using Faactory.Channels.Examples;
using Faactory.Channels.WebSockets;
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
    channel.AddInputAdapter<WordAdapter>()
        .AddInputHandler<WordHandler>();

    // set up output pipeline
    //channel.AddOutputAdapter<UTFEncoderAdapter>();
} );

/*
Configure Kestrel on port 8080
*/
builder.Services.Configure<KestrelServerOptions>( options =>
{
    options.ListenAnyIP( 8080 );
} );

var app = builder.Build();

app.UseWebSockets();

app.Map( "/ws/connect", async ( HttpContext httpContext, IWebSocketChannelFactory channelFactory ) =>
{
    // make sure this is a WebSocket request
    if ( !httpContext.WebSockets.IsWebSocketRequest )
    {
        httpContext.Response.StatusCode = 400;

        return;
    }

    // accept the WebSocket connection
    using var ws = await httpContext.WebSockets.AcceptWebSocketAsync();

    // create a WebSocket channel using the factory
    var channel = await channelFactory.CreateChannelAsync( ws );

    /*
    this creates a linked token source that will cancel when either of the following happens:
    - the HTTP request is aborted
    - the application is gracefully stopping

    if we used only the HTTP request token, the application would take longer to stop
    since the channel wouldn't receive the stop signal, leading to a delay in the application shutdown
    and a forced termination of the channel after the graceful shutdown timeout.
    */
    var cts = CancellationTokenSource.CreateLinkedTokenSource(
        [
            httpContext.RequestAborted,
            httpContext.RequestServices.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping
        ]
    );

    // wait until the channel is closed
    await channel.WaitAsync( cts.Token );

    // close the channel and release resources
    await channel.CloseAsync();
} );

app.Run();
