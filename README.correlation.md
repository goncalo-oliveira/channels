# Channels Correlation

Provides a lightweight request/response correlation mechanism for **Channels**.

It allows you to send a message through a channel and asynchronously wait for a specific response that matches a predicate.

This is useful for:

- Command/response protocols
- Acknowledgement flows
- Correlated messaging over TCP, UDP, or WebSockets

---

## Installation

```bash
dotnet add package Faactory.Channels.Correlation
```

---

## Registration

Correlation is **scoped per channel**.  
Since Channels creates a service scope per channel instance, each channel gets its own isolated registry.

```csharp
services.AddChannels( channel =>
{
    channel.AddCorrelation();

    // ...
} );
```

---

## Basic Usage

Inject `IChannelResponseRegistry` into your adapter or handler to feed incoming messages into the correlation system:

```csharp
public class MyMessageHandler( IChannelResponseRegistry registry ) : ChannelHandler<Message>
{
    public override Task ExecuteAsync( IChannelContext context, Message message, CancellationToken cancellationToken )
    {
        // Feed incoming messages into the registry
        registry.Push( message );

        return Task.CompletedTask;
    }
}
```

---

## Waiting for a Response

Correlation is intended to be used **outside of middleware**, from components that initiate a request and expect a response.

Typical usage scenarios include:

- Channel services
- Application-level services
- Client-side request/response flows

### From a Channel Service

Inside a `ChannelService`, the registry can be injected directly:

```csharp
public class MyService( IChannelResponseRegistry registry ) : ChannelService
{
    protected override async Task ExecuteAsync( CancellationToken cancellationToken )
    {
        var awaiter = registry.Create<Message>( m => m.Id == expectedId );

        await Channel.WriteAsync( request );

        var response = await awaiter.WaitAsync( cancellationToken );
    }
}
```

### From Outside the Channel Scope

If you only have an `IChannel` instance, retrieve the registry from the channel:

```csharp
var registry = channel.GetChannelResponseRegistry();

var awaiter = registry.Create<Message>( m => m.Id == expectedId );

await channel.WriteAsync( request );

var response = await awaiter.WaitAsync( cancellationToken );
```

The awaiter completes when:

- A pushed message matches the predicate  
- Or the cancellation token is canceled  

> [!NOTE]
> If canceled, `WaitAsync` throws `OperationCanceledException`.

---

## Behavior and Guarantees

### One-shot Awaiters

Each awaiter completes only once.  
After completion (success or cancellation), it is automatically removed.

### Fanout Semantics

If multiple awaiters match the same message, **all matching awaiters complete**.

### Cancellation

Cancellation behaves like any standard .NET async API:

```csharp
using var cts = new CancellationTokenSource( TimeSpan.FromSeconds( 5 ) );

try
{
    var response = await awaiter.WaitAsync( cts.Token );
}
catch ( OperationCanceledException )
{
    // Timeout or manual cancellation
}
```

### Channel Isolation

The registry is registered as **scoped**, ensuring:

- Correlation is isolated per channel instance  
- No cross-channel interference  
- Automatic cleanup when the channel scope ends  

---

## When to Use Correlation

Use correlation when:

- A request expects a specific reply  
- Messages contain identifiers or correlation keys  
- You need structured request/response behavior over streaming transports  

---

## Summary

Channels Correlation provides:

- Predicate-based matching  
- Cancellation-aware awaiting  
- Thread-safe completion  
- Scoped isolation per channel  
- Minimal overhead  

It integrates naturally with the Channels pipeline while keeping request/response coordination outside middleware.
