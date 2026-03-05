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

### Renting Buffers from a Pool

For scenarios that create many temporary writable buffers (for example when encoding messages), the library provides `ByteBufferPool`.

The pool rents buffers backed by reusable `byte[]` instances, reducing allocations and GC pressure.

Example:

```csharp
var pool = new ByteBufferPool();

using var buffer = pool.Rent();

buffer.WriteInt32( 123 );
buffer.WriteByte( 1 );

var readable = buffer.AsReadableView();
```

> [!IMPORTANT]
> Buffers rented from the pool **must be disposed** when no longer needed so the underlying byte array can be returned to the pool.

You can also request a specific minimum capacity:

```csharp
using var buffer = pool.Rent( 4096 );
```

The returned buffer behaves exactly like a regular `WritableByteBuffer`. If additional capacity is required while writing, the buffer will automatically grow as needed, just like a regular writable buffer. When this happens, the previously rented array is returned to the pool and a larger one is rented.

When disposed:
- the underlying byte array is returned to the pool
- the buffer instance becomes unusable

### Tracked Buffer Pool

For scenarios where many buffers are rented within a defined scope, the library also provides `TrackedByteBufferPool`.

This pool tracks all rented buffers and automatically disposes any that were not manually released when the pool itself is disposed.

Example:

```csharp
using var pool = new TrackedByteBufferPool();

var buffer1 = pool.Rent();
var buffer2 = pool.Rent();

buffer1.WriteInt32( 123 );

// disposing the pool automatically disposes any remaining buffers
```

This can be useful when buffers are created throughout a workflow and manual disposal could be easily forgotten.

> [!NOTE]
> If a rented buffer is disposed manually, it is automatically removed from the tracked set.

## Buffer Views (Zero-Copy)

Buffers in v2.x support **windowed views**, allowing efficient access to subsets of data without copying.

### Windowed Readable Buffers

`ReadableByteBuffer` can represent:

- The entire underlying `byte[]` or
- A **windowed view** over a portion of a `byte[]`

Windowed views are also created internally when using:

- `GetByteBuffer( offset, length )`
- `ReadByteBuffer( length )`

These methods **do not allocate**.  
Instead, they create a *readable view* over the same underlying array.

This significantly reduces allocations in high-throughput scenarios.

> [!IMPORTANT]
> A windowed view shares the underlying array.
> If you need to persist the data beyond the lifetime of the original buffer,
> call `ToArray()` to create a copy.

### `AsReadableView()`

`IWritableByteBuffer.AsReadableView()` creates a readable view over the **currently written portion** of the writable buffer.

- No allocation occurs.
- The returned buffer shares the underlying array.
- The readable view is valid as long as the writable buffer is not modified or compacted.

Example:

```csharp
var writable = new WritableByteBuffer();

writable.WriteInt32( 123 );

var readable = writable.AsReadableView(); // zero-copy
```

> [!WARNING]
> The readable view reflects the writable buffer’s current contents.
> If the writable buffer is modified, compacted, or resized, the view may no longer represent the same logical data.
>
> If a stable snapshot is required, create a copy:
>
> ```csharp
> var snapshot = readable.ToArray();
> ```

### `ToArray()` Behavior

`ToArray()` on a readable buffer returns:

- The backing array **if the readable buffer owns it entirely**
- A **copy** if the readable buffer is a windowed view

This ensures:
- No unnecessary allocations for full buffers
- Safe behavior for windowed or writable-backed views

### Summary of Allocation Behavior

| Operation | Allocates? |
|------------|------------|
| `GetByteBuffer` | ❌ No |
| `ReadByteBuffer` | ❌ No |
| `AsReadableView` | ❌ No |
| `AsReadable` | ✅ Yes |
| `AsWritable` | ✅ Yes |
| `ToArray()` (full-owned buffer) | ❌ No |
| `ToArray()` (windowed view) | ✅ Yes |

These improvements significantly reduce allocations in decoding pipelines and high-throughput scenarios while maintaining safety when snapshots are required.

## Good to know...

- `ReadableBytes` exposes how many bytes remain unread.
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
