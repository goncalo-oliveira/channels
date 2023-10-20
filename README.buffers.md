# Channels - Buffers

A simple library to handle buffer reading and writing. Although this library was designed to work with Channels and is part of the same project, it does not depend on it and therefore, can be used independently.

## Design

An important fact on buffers, is that they were designed to work exclusively for reading or writing, not both. So, a buffer that `IsReadable == true` will be `IsWritable == false` and the other way around. This was a design decision and applies to both `WrappedByteBuffer` and `WritableByteBuffer`.

It is possible to interchange this behaviour by creating a writable buffer from a readable one and vice-versa, by using the `MakeReadOnly` and `MakeWritable` extension methods.

## Using Buffers

When using Channels, you won't usually need to manually create a buffer instance, since you can interchange `Byte[]` and `IByteBuffer` types in the adapters. Nonetheless, if required, you can use `WritableByteBuffer` to create a writable buffer and a `WrappedByteBuffer` to create a readable buffer from a `Byte[]` object.

When reading data from a buffer, we have two distinct ways of doing so: by *getting* a value or by *reading* a value.

*Getting* a value from the buffer does not change the current reader offset, but *reading* a value, advances the offset by the number of bytes read.

```csharp
IByteBuffer buffer = ...;

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
