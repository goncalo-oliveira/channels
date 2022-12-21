# Channels

A TCP communication library based on middleware components.

![dotnet workflow](https://github.com/goncalo-oliveira/channels/actions/workflows/dotnet.yml/badge.svg)

## Design

The concept behind this library is to apply a *middleware pipeline* to data coming in and out from the open communication channels.

For data coming through the channel input, two middleware components can be applied: *adapters* and *handlers*.

```mermaid
graph LR;
    channelInput((Input)) --> a1[/Adapter/]
    subgraph adapters
    a1 --> a2[/Adapter/]
    end
    a2 --> Handlers
```

For data going through the channel output, only *adapters* are applicable.

```mermaid
graph LR;
    openChannel((Channel)) --> a1[/Adapter/]
    subgraph adapters
    a1 --> a2[/Adapter/]
    end
    a2 --> channelOutput([Output])
```

## Adapters

An adapter is a middleware component that can be executed at any point in the pipeline and it has a single conceptual purpose: to adapt data.

```mermaid
graph LR;
    in[/Data In/] --> Adapter --> out[/Data Out/]
```

An adapter is expected to *forward* data to next component in the pipeline, although that is not mandatory. However, if the adapter doesn't forward any data, the pipeline is interrupted and no other components will be executed.

### Implementing an Adapter

Unless you have very specific needs, you should inherit your adapter from the `ChannelAdapter<T>` abstract class and not implementing the `IChannelAdapter` interface directly. This is because the base class does a few things for us that, if implementing the interface directly, won't be available. That includes

- Type checking
- Type mutation

Type checking is essentially making sure the type of the data is intended for an adapter. If it's not, data is automatically forwarded to the next middleware component in the pipeline.

Type mutation is the capacity to change the data type, if compatible with the expected data of the adapter. The base class already deals with `IByteBuffer` <--> `Byte[]` and `T` <--> `IEnumerable<T>` mutations, but it also provides an opportunity to override/extend this behaviour.

Since version 0.3 we also need to indicate whether the adapter is meant for the input or/and the output pipelines. We do that by adding the interfaces `IInputChannelAdapter` or/and `IOutputChannelAdapter` respectively.

Here's an example of how to implement an adapter that adapts from an `IByteBuffer` (or `Byte[]`). This adapter can only be added to the input pipeline.

```csharp
public class MyChannelAdapter : ChannelAdapter<IByteBuffer>, IInputChannelAdapter
{
    public override Task ExecuteAsync( IAdapterContext context, IByteBuffer data )
    {
        // adapt/transform data
        var adaptedData = ...

        // forward adapted data
        context.Forward( adaptedData );
    }
}
```

### Ready-made Adapters

In addition to the abstract `ChannelAdapter<T>` adapter, you have a few more ready-made adapters that you can use.

| Adapter                 | Target       | Description                                        |
|-------------------------|--------------|----------------------------------------------------|
| AnonymousChannelAdapter | Input/Output | A quick way to implement an anonymous adapter      |
| BufferLengthAdapter     | Input        | Ensures the input buffer doesn't exceed in length  |

## Handlers

Although handlers are very similar to adapters, their conceptual purpose is different: to handle data. That means that business logic should be applied here and not on an adapter. Also, handlers are executed at the end of the pipeline and as such, they don't forward data.

```mermaid
graph LR;
    in[/Data In/] --> Handler
```

### Implementing an Handler

Similarly to the adapter, unless you have very specific needs, you should inherit your handler from the `ChannelHandler<T>` class and not implementing the `IChannelHandler` interface directly. This is because, again, similarly to the adapter, the base class does type checking for us; if the data type is not intended for the handler, then it (the handler) won't be executed.

Similarly to the adapter, the base class also deals with `T` <--> `IEnumerable<T>` mutations.


```csharp
public class MyChannelHandler : ChannelHandler<MyData>
{
    public override Task ExecuteAsync( IChannelContext context, MyData data )
    {
        // implement your handler here
    }
}
```

## Adapters vs Handlers

Because adapters and handlers are so similar, there might be a temptation to do everything with adapters. And while that's feasable, it's not recommended. Adapters should be used to adapt data and handlers to handle data (business logic).

- Adapters adapt and forward data
- Handlers handle data and business logic
- Adapters can run at any point in the pipeline
- Handlers run at the end of the pipeline

## Writing to Channel Output

At any point, within an adapter or handler, we can write data to the channel output; this will trigger the output pipeline and at the end of it, send the data to the other party. However, there are two distinct ways of doing this, each with a distinct behaviour.

### 1. Write directly to the Channel

This is the most straightforward method and it will immediately trigger the output pipeline, however, it is **not** the recommended way, unless you need the data to reach the other party as soon as possible, no matter what happens next (current or next middleware component). This is an asynchronous process.

```csharp
public override async Task ExecuteAsync( IAdapterContext context, IEnumerable<Message> data )
{
    // ...

    await context.Channel.WriteAsync( replyData );
}
```

### 2. Write to the Output buffer (recommended)

The context of an adapter or handler, gives us access to an output buffer that we can write to. This **is** the recommended method. Writing to the output buffer doesn't immediately trigger the output pipeline, instead, it is only triggered at the end of the pipeline, after all adapters and handlers have executed - without interruption.
If an adapter interrupts the pipeline, or a handler crashes and interrupts the pipeline, the data in the buffer will never be written to the channel.

```csharp
public override async Task ExecuteAsync( IAdapterContext context, IEnumerable<Message> data )
{
    // ...

    context.Output.Write( replyData );
}
```

## Getting Started

Install the package from NuGet

```bash
dotnet add package Faactory.Channels
```

To quickly bootstrap a server, we need an `HostBuilder` to inject a *hosted service*. Then we need to configure the listening options and set up the input and output pipelines. Here's an example

```csharp
var builder = new HostBuilder()
    .ConfigureServices( ( context, services) =>
    {
        // add logging
        services.AddLogging( loggingBuilder =>
        {
            loggingBuilder.AddConsole()
                .SetMinimumLevel( LogLevel.Debug );
        } );

        // add our hosted service
        services.AddChannelsHostedService( builder =>
        {
            // configure options
            builder.Configure( options =>
            {
                options.Port = 8080;
                options.Backlog = 30;
            } );

            // set up input pipeline
            builder.AddInputAdapter<ExampleDecoderChannelAdapter>()
                .AddInputHandler<MyChannelHandler>();

            // set up output pipeline
            builder.AddOutputAdapter<ExampleEncoderAdapter>();
        } );
    } )
    .UseConsoleLifetime();

await builder.Build().RunAsync();
```

To boostrap the client, we'll need to register the factory with a service provider. Then, similarly to the server, we need to configure the channel options and set up the input and output pipelines. Here's an example

```csharp
IServiceCollection services = ...

// add logging
services.AddLogging( loggingBuilder =>
{
    loggingBuilder.AddConsole()
        .SetMinimumLevel( LogLevel.Debug );
} );

// add our client factory
services.AddChannelsClient( builder =>
{
    // configure options
    builder.Configure( options =>
    {
        options.Host = "localhost";
        options.Port = 8080;
    } );
    
    // set up input pipeline
    builder.AddInputAdapter<ExampleDecoderChannelAdapter>()
        .AddInputHandler<MyChannelHandler>();

    // set up output pipeline
    builder.AddOutputAdapter<ExampleEncoderAdapter>();
} );

var provider = services.BuildServiceProvider();
var channelFactory = provider.GetRequiredService<IClientChannelFactory>();
var channel = await channelFactory.CreateAsync();

await channel.WriteAsync( new MyData
{
    // ...
} );
```

## Adapters and Buffers

Although raw data handling in the adapters can be done with `Byte[]`, it is recommended to use an `IByteBuffer` instance instead, particularly for reading data. You can read more about it [here](src/buffers/README.md).

## Service Scope

Every channel instance (client or service) uses a new `IServiceScope`. This means that if you add a scoped service to the DI container and use it in an adapter or handler, you'll have a unique instance per channel.

## Channel Events

In some cases, you might need to tap into channel events. This can be useful for logging, statistics or a custom scenario. The following events are available

- Channel Created
- Channel Closed
- Data Received
- Data Sent

To receive channel events, you'll need to create a class that implements `IChannelEvents` interface and then add it to the DI container. You can have multiple implementations.

```csharp
public class MyChannelEvents : IChannelEvents
{
    // ...
}

// ...

IServiceCollection services = ...;

services.AddTransient<IChannelEvents, MyChannelEvents>();
```

## Idle Channels

An idle detection mechanism is available by default for both service and client channels. The default detection method is `IdleDetectionMode.Auto`. This mode attempts to verify if the underlying socket is still connected and if not, closes the channel.
By default, there's also an hard timeout of 60 seconds; if no data is received or sent through the underlying socket before the timeout, the channel is closed. Using the `IdleDetectionMode.Auto` method the hard timeout can be disabled by setting its value to `TimeSpan.Zero` in the channel options.

```csharp
IServiceChannelBuilder builder = ...;

builder.Configure( options =>
{
    options.Port = 8080;

    // these are the default settings; added here just for clarity
    options.IdleDetectionMode = IdleDetectionMode.Auto;
    options.IdleDetectionTimeout = TimeSpan.FromSeconds( 60 );
    // to use Auto method without the hard timeout
    //options.IdleDetectionTimeout = TimeSpan.Zero;
} );
```


Other detection modes only use the hard timeout on received or sent data, or both.

```csharp
IServiceChannelBuilder builder = ...;

builder.Configure( options =>
{
    options.Port = 8080;

    // hard timeout (30s) to both received and sent data
    options.IdleDetectionMode = IdleDetectionMode.Both;
    options.IdleDetectionTimeout = TimeSpan.FromSeconds( 30 );
} );
```

The idle detection can be disabled by setting the mode to `IdleDetectionMode.None`.

```csharp
IServiceChannelBuilder builder = ...;

builder.Configure( options =>
{
    options.Port = 8080;

    // disable idle detection
    options.IdleDetectionMode = IdleDetectionMode.None;
} );
```

## Protocol Extensions

The following are available as protocol extensions built for Channels.

- [Parcel](https://github.com/goncalo-oliveira/channels-parcel)
- [Teltonika](https://www.nuget.org/packages/Faactory.Channels.Teltonika)
