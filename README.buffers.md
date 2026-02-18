# Channels - Buffers

A simple library to handle buffer reading and writing. Although this library was designed to work with Channels and is part of the same project, it does not depend on it and therefore, can be used independently.

## Design

An important fact that needs to be known from the start, is that buffer instances are always specialized in either reading or writing, never both. For reading, we should always work with `IReadableByteBuffer` instances, and for writing, we should always work with `IWritableByteBuffer` instances. This design decision allowed us to optimize the implementation of both buffer types.

> [!TIP]
> It's always possible to create a writable buffer from a readable one and vice-versa, by using the `AsReadable` and `AsWritable` extension methods.

## Using Buffers

When using Channels, we won't usually need to manually create a buffer instance, since we can interchange `byte[]` and `IReadableByteBuffer` types in the adapters. Nonetheless, if required, or outside of the context of Channels, we can use `WritableByteBuffer` to create a writable buffer and a `ReadableByteBuffer` to create a readable buffer from a `byte[]` object.

When reading data from a buffer, we have two distinct ways of doing so: by *getting* a value or by *reading* a value.

*Getting* a value from a buffer does not change the current reader offset, but *reading* a value, advances the offset by the number of bytes read.

```csharp
IReadableByteBuffer buffer = ...;

// reads a byte at 'customOffset' without changing the buffer's offset
var b1 = buffer.GetByte( customOffset );

// reads a byte at the buffer's current offset and moves the offset 1 byte forward
var b2 = buffer.ReadByte();
```

## Good to know...

- The remaining readable bytes in the buffer are exposed by the `ReadableBytes` property.
- Invoking `DiscardReadBytes` discards all read bytes (`ReadXXX` methods) and resets the offset
- Invoking `ResetOffset` undoes previous readings by resetting the offset
- Invoking `ToArray` returns the entire buffer no matter where the offset is
- Except for the *getting* and *reading* methods, the buffer interface uses a fluent design

## Migrating from v1.x

Earlier versions of the library had a single `IByteBuffer` interface that was used for both reading and writing. This relied on checking if the buffer was readable/writable before performing the corresponding operation or catching a non-readable/writable exception.

The interface `IByteBuffer` still exists in v2.x, but it now contains only the common properties and methods of both buffer types. The `IReadableByteBuffer` and `IWritableByteBuffer` interfaces now inherit from `IByteBuffer` and contain the reading and writing methods, respectively, resulting in a much clearer API.

When coming from v1.x, the `IByteBuffer` interface should be replaced by either `IReadableByteBuffer` or `IWritableByteBuffer`, depending on the use case. Most of the read/write operations are the same, with one or two exceptions, so the migration should be straightforward.

If you have middleware that expects a `IByteBuffer` instance, these need to be replaced by `IReadableByteBuffer`. Writable buffers aren't expected in the middleware and conversions from `byte[]` to `IWritableByteBuffer` are not supported.
