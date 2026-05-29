namespace Faactory.Channels.Buffers;

/// <summary>
/// A buffer handling interface with write capabilities
/// </summary>
[Serialization.ByteBufferJsonConverter]
public interface IWritableByteBuffer : IByteBuffer, IDisposable
{
    /// <summary>
    /// Gets the total capacity of the buffer, which may be greater than or equal to the length of the currently written portion.
    /// </summary>
    int Capacity { get; }

    /// <summary>
    /// Creates a readable view of the currently written portion of the buffer.
    /// </summary>
    /// <remarks>
    /// Returns a zero-copy readable view over the currently written portion of the buffer.
    /// The returned view shares the same underlying memory.
    /// It must not be used after the writable buffer is modified (e.g., written to or compacted),
    /// as the view may no longer represent the same logical data.
    /// </remarks>
    /// <returns>A readable buffer view of the currently written portion</returns>
    IReadableByteBuffer AsReadableView();

    /// <summary>
    /// Gets the used portion of the buffer as a <see cref="Span{T}"/>
    /// </summary>
    /// <returns>A <see cref="Span{T}"/> representing the used portion of the buffer</returns>
    new Span<byte> AsSpan();

    /// <summary>
    /// Discards all written bytes and reallocates the buffer to its initial capacity.
    /// </summary>
    /// <returns>The same IWritableByteBuffer instance to allow fluent syntax</returns>
    IWritableByteBuffer Clear();

    /// <summary>
    /// Reduces the underlying buffer capacity when it significantly exceeds the configured maximum retained capacity.
    /// </summary>
    /// <returns>The same IWritableByteBuffer instance to allow fluent syntax</returns>
    IWritableByteBuffer Compact();

    /// <summary>
    /// Creates a writable view over the specified region of the buffer.
    /// The returned view shares the same underlying memory, allowing for zero-copy modifications.
    /// The offset must be within the bounds of the buffer's capacity.
    /// Modifying the returned view will affect the original buffer, and vice versa.
    /// The returned view is limited to the portion of the buffer starting from the specified offset to the length of the used portion of the buffer.
    /// </summary>
    /// <param name="offset">The offset from which to create the writable view</param>
    /// <param name="length">The length of the writable view</param>
    /// <returns>A writable buffer view starting at the specified offset</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the offset is negative or greater than the used portion of the buffer, or when the length is negative or extends beyond the used portion of the buffer</exception>
    IWritableByteBufferView CreateView( int offset, int length );

    /// <summary>
    /// Rebases the buffer so that the specified offset becomes the new beginning of the buffer.
    /// </summary>
    /// <param name="offset">The offset to rebase the buffer to</param>
    /// <returns>The same IWritableByteBuffer instance to allow fluent syntax</returns>
    IWritableByteBuffer Rebase( int offset );

    /// <summary>
    /// Reserves a contiguous block of bytes for writing and moves the writing offset forward by the specified length.
    /// </summary>
    /// <param name="length">The number of bytes to reserve for writing</param>
    /// <returns>The same IWritableByteBuffer instance to allow fluent syntax</returns>
    IWritableByteBuffer Reserve( int length );

    /// <summary>
    /// Truncates the buffer to the specified position, effectively discarding all written bytes beyond that point. Underlying buffer capacity remains unchanged.
    /// </summary>
    /// <param name="offset">The offset to truncate to; defaults to the beginning of the buffer</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the offset is negative or greater than the used portion of the buffer</exception>
    /// <returns>The same IWritableByteBuffer instance to allow fluent syntax</returns>
    IWritableByteBuffer Truncate( int offset = 0 );

    /// <summary>
    /// Writes a boolean value
    /// </summary>
    /// <param name="value">The value to write</param>
    /// <returns>The same IWritableByteBuffer instance to allow fluent syntax</returns>
    IWritableByteBuffer WriteBoolean( bool value );

    /// <summary>
    /// Writes a byte value
    /// </summary>
    /// <param name="value">The value to write</param>
    /// <returns>The same IWritableByteBuffer instance to allow fluent syntax</returns>
    IWritableByteBuffer WriteByte( byte value );

    /// <summary>
    /// Writes a range of bytes
    /// </summary>
    /// <param name="value">The value to write</param>
    /// <param name="startIndex">The index in the value to start writing from</param>
    /// <param name="length">The number of bytes to write</param>
    /// <returns>The same IWritableByteBuffer instance to allow fluent syntax</returns>
    IWritableByteBuffer WriteBytes( byte[] value, int startIndex, int length );

    /// <summary>
    /// Writes a range of bytes from a <see cref="ReadOnlySpan{T}"/>
    /// </summary>
    /// <param name="value">The value to write</param>
    /// <returns>The same IWritableByteBuffer instance to allow fluent syntax</returns>
    IWritableByteBuffer WriteBytes( ReadOnlySpan<byte> value );

    /// <summary>
    /// Writes a double value
    /// </summary>
    /// <param name="value">The value to write</param>
    /// <returns>The same IWritableByteBuffer instance to allow fluent syntax</returns>
    IWritableByteBuffer WriteDouble( double value );

    /// <summary>
    /// Writes a float value
    /// </summary>
    /// <param name="value">The value to write</param>
    /// <returns>The same IWritableByteBuffer instance to allow fluent syntax</returns>
    IWritableByteBuffer WriteSingle( float value );

    /// <summary>
    /// Writes a short value
    /// </summary>
    /// <param name="value">The value to write</param>
    /// <returns>The same IWritableByteBuffer instance to allow fluent syntax</returns>
    IWritableByteBuffer WriteInt16( short value );

    /// <summary>
    /// Writes an int value
    /// </summary>
    /// <param name="value">The value to write</param>
    /// <returns>The same IWritableByteBuffer instance to allow fluent syntax</returns>
    IWritableByteBuffer WriteInt32( int value );

    /// <summary>
    /// Writes a long value
    /// </summary>
    /// <param name="value">The value to write</param>
    /// <returns>The same IWritableByteBuffer instance to allow fluent syntax</returns>
    IWritableByteBuffer WriteInt64( long value );

    /// <summary>
    /// Writes an ushort value
    /// </summary>
    /// <param name="value">The value to write</param>
    /// <returns>The same IWritableByteBuffer instance to allow fluent syntax</returns>
    IWritableByteBuffer WriteUInt16( ushort value );

    /// <summary>
    /// Writes an uint value
    /// </summary>
    /// <param name="value">The value to write</param>
    /// <returns>The same IWritableByteBuffer instance to allow fluent syntax</returns>
    IWritableByteBuffer WriteUInt32( uint value );

    /// <summary>
    /// Writes an ulong value
    /// </summary>
    /// <param name="value">The value to write</param>
    /// <returns>The same IWritableByteBuffer instance to allow fluent syntax</returns>
    IWritableByteBuffer WriteUInt64( ulong value );    
}
