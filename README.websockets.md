# Channels over WebSockets

An extension library to use the Channels middleware with WebSockets.

## Design

Unlike TCP or UDP Channels, the library does not provide a low-level server to handle WebSocket connections. Instead, it provides a factory to create a Channel on top of an existing WebSocket connection. It also provides extension methods to make it super easy to integrate with the ASP.NET Core middleware.

Unlike the TCP and UDP Channels, the WebSockets middleware does not deliver raw byte buffers to the input pipeline. Instead, it delivers a `WebSocketMessage` instance, which contains the message type and payload, since web sockets support both binary and text messages.

> [!TIP]
> Messages delivered to the input pipeline are always complete messages (`WebSocketMessage.EndOfMessage: true`). If a fragmented message is received, the channel will buffer the fragments until it is complete and only then deliver it to the input pipeline.

When writing to the output pipeline, the library provides built-in middleware for sending binary, text or fragmented messages, so either of the following types can be directly written to the output pipeline:

- `byte[]` which is sent as a (complete) binary message
- `IByteBuffer` also sent as a (complete) binary message
- `string` which is sent as a (complete) text message (utf-8 encoded)
- `WebSocketMessage` which gives you full control over the message type and content

## Usage

To make use of the library, you first need to add the NuGet package to your project:

```bash
dotnet add package Faactory.Channels.WebSockets
```

The channel pipeline configuration is done in the same way as with the TCP and UDP channels; WebSocket channels will use this same configuration. Nonetheless, additional services are required to set up the web sockets middleware.

```csharp
IServiceCollection services = ...;

/*
Configure the channel or channels as usual
*/
services.AddChannels( ... );

/*
register web sockets middleware services
*/
services.AddWebSocketChannels();
```

With the middleware in place, we now need to bind the web sockets endpoint to the middleware, which is done through route mapping:

```csharp
WebApplication app = ...;

// required WebSockets middleware
app.UseWebSockets();

// map the web socket endpoint to the Channels default pipeline
app.MapWebSocketChannel( "/ws" );
```

## Multiple Endpoints

It is possible to have multiple web sockets endpoints with different Channels pipeline configuration. This can be achieved by using named channels:

```csharp
IServiceCollection services = ...;

services.AddChannels()
    .Add( "channel1", channel =>
    {
        // set up input pipeline
        channel.AddInputAdapter<ExampleAdapter>()
            .AddInputHandler<ExampleHandler1>();

        // set up output pipeline
        channel.AddOutputAdapter<ExampleAdapter>();
    } )
    .Add( "channel2", channel =>
    {
        // set up input pipeline
        channel.AddInputAdapter<ExampleAdapter>()
            .AddInputHandler<ExampleHandler2>();

        // set up output pipeline
        channel.AddOutputAdapter<ExampleAdapter>();
    } );
```

The above configuration sets up two named pipelines, each with its own middleware. To bind the named pipelines to the web sockets endpoints, you now need to include the pipeline name in the route mapping:

```csharp
WebApplication app = ...;

// required WebSockets middleware
app.UseWebSockets();

// map the web socket endpoints
app.MapWebSocketChannel( "/ws/foo", "channel1" );
app.MapWebSocketChannel( "/ws/bar", "channel2" );
```

## Usage without ASP.NET Core

If you are not using ASP.NET Core, you can still use the library with any HTTP server that can produce a `System.Net.WebSockets.WebSocket` instance. In this case, you won't be using the `MapWebSocketChannel` extension method, but instead, you will need to manually create the channel by using the factory.

```csharp
WebSocket webSocket = ...;
IWebSocketChannelFactory factory = ...; // get the factory from the DI container
CancellationToken cancellationToken = ...; // optional: graceful shutdown if using the WaitAsync method

// create named channel using the pre-configured "channel1" pipeline
var channel = factory.CreateChannel( webSocket, "channel1" );

// optional: wait until the channel is closed or cancellation token is triggered
try
{
    await channel.WaitAsync( cts.Token );
}
catch ( OperationCanceledException )
{ }

// recommended: close the channel and release resources
await channel.CloseAsync();
```
