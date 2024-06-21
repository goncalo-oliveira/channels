using Faactory.Channels;
using Faactory.Channels.Adapters;
using Faactory.Channels.Examples;
using Faactory.Channels.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder( args );

builder.Services.AddUdpListener();
builder.Services.AddKeyedTransient<IInputChannelAdapter, WordAdapter>( "__default" );
builder.Services.AddKeyedTransient<IChannelHandler, WordHandler>( "__default" );
builder.Services.AddKeyedTransient<IOutputChannelAdapter, UTFEncoderAdapter>( "__default" );

// builder.Services.Configure<ServiceChannelOptions>( options =>
// {
//     options.Port = 8080;
//     options.Backlog = 10;
//     options.TransportMode = ChannelTransportMode.Udp;    
// } );

// builder.Services.AddChannels( channel =>
// {
//     // configure options
//     channel.Configure( options =>
//     {
//         options.Port = 8080;
//         options.Backlog = 10;
//     } );

//     // set up long-running services
//     // since v0.5 idle monitoring is a channel service
//     channel.AddIdleChannelService();

//     // set up input pipeline
//     channel.AddInputAdapter<WordAdapter>()
//         .AddInputHandler<WordHandler>();

//     // set up output pipeline
//     channel.AddOutputAdapter<UTFEncoderAdapter>();
// } );

// builder.Services.AddChannelsClient()
//     .AddChannel( "client", channel =>
//     {
//         // configure options
//         channel.Configure( options =>
//         {
//             options.Host = "localhost";
//             options.Port = 8080;
//             options.ChannelOptions.TransportMode = ChannelTransportMode.Udp;
//         } );

//         // set up input pipeline
//         channel.AddInputHandler<ClientHandler>();

//         // set up output pipeline
//         channel.AddOutputAdapter<UTFEncoderAdapter>();
//     } );

// builder.Services.AddHostedService<ClientHostedService>(); // our client test service

var app = builder.Build();

app.Run();
