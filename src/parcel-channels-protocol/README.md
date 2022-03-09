# Parcel Channels - Protocol Extensions

Parcel Protocol extensions for the Parcel Channels library.

Learn more about [Parcel Protocol](https://github.com/goncalo-oliveira/parcel-spec).

## Getting Started

Install the package from NuGet

```bash
dotnet add package Parcel.Channels.Protocol
```

To enable decoding or encoding of Parcel Messages on the pipeline, we just need to register the respective adapters with the channel pipeline. It is the same for server or client channels.

```csharp
IChannelBuilder builder = ...

// This adapter will decode from a byte[] and forward a Parcel.Protocol.Message[]
builder.AddInputAdapter<ParcelDecoderAdapter>();

// User handler implementation to perform business logic
builder.AddInputHandler<MyMessageHandler>();

// This adapter will encode a Parcel.Protocol.Message or a Parcel.Protocol.Message[] into a byte[]
builder.AddOutputAdapter<ParcelEncoderAdapter>();
```

## Observables

It is possible to write to the output channel and then wait for a specific response on the input channel. This is useful if we know that the server will reply with a Parcel Message with a specific identifier.

To make this work, we first need to register the `IMessageObserver` singleton; this will allow us to retrieve the instance through dependency injection.

```csharp
IChannelBuilder builder = ...

builder.AddMessageObserver();
```

With the message observer registered, we just need to tell it that we want to wait for a message with a particular identifier. Here's an example

```csharp
IChannel channel = ...
IMessageObserver observer = ...

// for our example, we know that sending this message
// will result in a reply with the identifier 'my-reply'
await channel.WriteAsync( new Message
{
    // ...
} );

// so we tell the observer just that
var replyMessage = await observer.WaitForAsync( "my-reply" );

if ( replyMessage == null )
{
    // the response is null if the timeout triggers
}
```

This isn't enough... Because the handling of the messages is done by our pipeline and by our handler, we need to tell the observer when we receive messages. For this example, we'll do it on our `MyMessageHandler` handler.

```csharp
public class MyMessageHandler : ChannelHandler<IEnumerable<Message>>
{
    private IMessageObserver observer;

    public MyMessageHandler( IMessageObserver messageObserver )
    {
        // our observer is injected
        observer = messageObserver;
    }

    public override Task ExecuteAsync( IChannelContext context, IEnumerable<Message> data )
    {
        foreach ( var message in data )
        {
            // ...

            // let the observer know we received a message
            observer.Push( message );
        }
        // ...
    }
}
```
