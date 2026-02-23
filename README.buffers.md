# Channels - Buffers

A lightweight library for reading and writing byte buffers.

Although originally designed for *Channels* and part of the same project, this package has no dependency on *Channels* and can be used independently.

## Design

Buffer instances are always specialized for either reading or writing, never both.
- Use `IReadableByteBuffer` for reading
- Use `IWritableByteBuffer` for writing

This separation allows each implementation to be optimized for its specific purpose.

> [!TIP]
> You can convert between buffer types using the `AsReadable` and `AsWritable` extension methods.

## Using Buffers

When using *Channels*, you usually don’t need to manually create buffer instances. The pipeline can automatically convert between `byte[]` and `IReadableByteBuffer`.

Outside of Channels, or when needed explicitly:
- Use `WritableByteBuffer` to create a writable buffer.
- Use `ReadableByteBuffer` to wrap a `byte[]`.

### Reading Data

There are two ways to retrieve data:

**Get vs Read**
- **Get** does not change the current offset
- **Read** advances the offset

```csharp
IReadableByteBuffer buffer = ...;

// Reads at a custom offset (does not change buffer offset)
var b1 = buffer.GetByte(customOffset);

// Reads at current offset and advances it by 1
var b2 = buffer.ReadByte();
```

## Good to know...

- `ReadableBytes` exposes how many bytes remain unread.
- `DiscardReadBytes` removes already-read bytes and resets the offset.
- `ResetOffset` resets the read offset (undoes reads).
- `ToArray` returns the entire buffer, regardless of offset.
- Except for reading/getting methods, the API follows a fluent design.
- `IWritableByteBuffer` is not expected in middleware.

## Output Pipeline Notes

While middleware in the input pipeline is expected to use `IReadableByteBuffer`, the output pipeline is more permissive.

In the output pipeline:
- `byte[]`
- `IReadableByteBuffer`
- `IByteBuffer`
- `IWritableByteBuffer`

are all supported and ultimately treated as `byte[]` before being written to the transport.

This allows adapters and handlers in the output pipeline to work with writable buffers when building outgoing payloads, while preserving the strict readable-only design in the input pipeline.

## Migrating from v1.x

In v1.x, a single `IByteBuffer` interface handled both reading and writing. Consumers had to:
- Check readability/writability
- Or handle runtime exceptions

In v2.x:
- `IByteBuffer` now contains only common members.
- `IReadableByteBuffer` and `IWritableByteBuffer` inherit from it.
- Read and write responsibilities are clearly separated.

### Migration Guidelines
- Replace `IByteBuffer` with:
- `IReadableByteBuffer` for reading scenarios
- `IWritableByteBuffer` for writing scenarios
- Most APIs remain the same, so migration is typically straightforward.
- Middleware should use `IReadableByteBuffer`.
- `IWritableByteBuffer` is not expected in middleware.
- Automatic conversion from `byte[]` to `IWritableByteBuffer` is not supported.
