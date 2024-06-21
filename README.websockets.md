# Channels over WebSockets

An extension library to use the Channels middleware with WebSockets.

## Design

Unlike TCP or UDP Channels, the library does not provide a low-level server to handle WebSocket connections. Instead, it provides a pipeline builder to create a Channel and its associated middleware over an existing WebSocket connection. While this could allow the library to be used with any HTTP server that can produce a WebSocket connection, it does provide extension methods that make it super easy to use with the ASP.NET Core middleware.

Unlike the TCP and UDP Channels, the WebSockets middleware does not deliver raw byte buffers to the input pipeline. Instead, it delivers a `WebSocketMessage` instance, which contains the message type and payload, since web sockets support both binary and text messages.

> [!TIP]
> Messages delivered to the input pipeline are always complete messages (`WebSocketMessage.EndOfMessage: true`). If a fragmented message is received, the channel will buffer the fragments until the message is complete and only then deliver it to the input pipeline.

When writing to the output pipeline, the library provides built-in middleware for sending binary, text or fragmented messages, so either of the following types can be directly written to the output pipeline:

- `byte[]` which is sent as a (complete) binary message
- `IByteBuffer` also sent as a (complete) binary message
- `string` which is sent as a (complete) text message
- `WebSocketMessage` which gives you full control over the message type and content

## Usage

To make use of the library, you first need to add the NuGet package to your project:

```bash
dotnet add package Faactory.Channels.WebSockets
```

If you are using a single web sockets endpoint, you can configure the Channels middleware mostly the same way you would with regular Channels, you just use a different extension method:

```csharp
IServiceCollection services = ...;

services.AddWebSocketChannels( channel =>
{
    /*
    configure options is minimal, since there's no server configuration
    */
    channel.Configure( options =>
    {
        options.BufferEndianness = Buffers.Endianness.BigEndian;
    } );

    /*
    set up input pipeline. remember that the first adapter should
    read a WebSocketMessage instance and not a IByteBuffer as with TCP channels
    */
    channel.AddInputAdapter<ExampleDecoderChannelAdapter>()
        .AddInputHandler<MyChannelHandler>();

    /*
    set up optional output pipeline
    */
    channel.AddOutputAdapter<ExampleEncoderAdapter>();
} );
```

With the middleware in place, you now need to bind the web sockets endpoint to the middleware, which is done through route mapping:

```csharp
WebApplication app = ...;

// required WebSockets middleware
app.UseWebSockets();

// map the web socket endpoint to the Channels middleware
app.MapWebSocketChannel( "/ws" );
```

## Multiple Endpoints

It is possible to have multiple web sockets endpoints with different Channels pipelines. This can be achieved by using named pipelines:

```csharp
IServiceCollection services = ...;

services.AddWebSocketChannels()
    .AddChannel( "foo", channel =>
    {
        channel.AddInputAdapter<ExampleAdapter1>()
            .AddInputHandler<ExampleHandler1>();
    } )
    .AddChannel( "bar", channel =>
    {
        channel.AddInputAdapter<ExampleAdapter2>()
            .AddInputHandler<ExampleHandler2>();
    } );
```

The above configuration sets up two named pipelines, each with its own middleware. To bind the named pipelines to the web sockets endpoints, you now need to include the pipeline name in the route mapping:

```csharp
WebApplication app = ...;

// required WebSockets middleware
app.UseWebSockets();

// map the web socket endpoints
app.MapWebSocketChannel( "/ws/foo", "foo" );
app.MapWebSocketChannel( "/ws/bar", "bar" );
```

## Usage without ASP.NET Core

If you are not using ASP.NET Core, you can still use the library with any HTTP server that can produce a `System.Net.WebSockets.WebSocket` instance. In this case, you won't be using the `MapWebSocketChannel` extension method, but instead, you will need to create the channel manually by using the factory.

```csharp
WebSocket webSocket = ...;
IWebSocketChannelFactory factory = ...; // get the factory from the DI container
CancellationToken cancellationToken = ...; // graceful shutdown if using the WaitAsync method

// create named channel using the pre-configured "foo" pipeline
var channel = factory.CreateChannel( webSocket, "foo" );

// optional: wait until the channel is closed or cancellation token is triggered
try
{
    await channel.WaitAsync( cts.Token );
}
catch ( OperationCanceledException )
{ }

// close the channel and release resources
await channel.CloseAsync();
```
