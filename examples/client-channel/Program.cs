using Faactory.Channels.Examples;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder( args );

builder.Logging.SetMinimumLevel( LogLevel.Debug );

/*
configure two different channel pipelines, one for the server and one for the client
*/
builder.Services.AddChannels()
    .Add( "server", channel =>
    {
        channel.AddInputAdapter<WordAdapter>()
            .AddInputHandler<WordHandler>();

        channel.AddOutputAdapter<UTFEncoderAdapter>();
    } )
    .Add( "client", channel =>
    {
        channel.AddInputHandler<ClientHandler>();

        channel.AddOutputAdapter<UTFEncoderAdapter>();
    } )
    ;

/*
Configure TCP server with the server channel pipeline
*/
builder.Services.AddTcpChannelListener( "server", 8080 );

/*
Configure the client with the client channel pipeline
*/
builder.Services.AddChannelsClient( "client", "tcp://localhost:8080" );

/*
this service will connect to the server, send a message
and wait for the server to respond.
*/
builder.Services.AddHostedService<ClientHostedService>();

var app = builder.Build();

app.Run();
