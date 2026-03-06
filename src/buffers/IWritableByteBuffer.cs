namespace Faactory.Channels.Buffers;

/// <summary>
/// A buffer handling interface with write capabilities
/// </summary>
[Serialization.ByteBufferJsonConverter]
public interface IWritableByteBuffer : IByteBuffer, IDisposable
{
    /// <summary>
    /// Gets the current writing offset, which indicates the position in the buffer where the next write operation will occur.
    /// This offset is automatically updated as data is written to the buffer.
    /// </summary>
    int Offset { get; }

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
    /// Compacts the buffer by discarding bytes up to the specified offset.
    /// </summary>
    /// <param name="offset">The offset up to which bytes should be discarded</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the offset is negative or greater than the used portion of the buffer</exception>
    /// <returns>The same IWritableByteBuffer instance to allow fluent syntax</returns>
    IWritableByteBuffer Compact( int offset );

    /// <summary>
    /// Discards all written bytes and reallocates the buffer to its initial capacity.
    /// </summary>
    /// <returns>The same IWritableByteBuffer instance to allow fluent syntax</returns>
    IWritableByteBuffer Clear();

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
    /// Moves the writing offset to the specified position, allowing for overwriting previously written bytes. The offset must be within the bounds of the buffer's capacity.
    /// </summary>
    /// <param name="offset">The position to move the writing offset to</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the offset is negative or greater than the used portion of the buffer</exception>
    /// <returns>The same IWritableByteBuffer instance to allow fluent syntax</returns>
    IWritableByteBuffer Seek( int offset );

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
    /// Writes the contents of an IByteBuffer instance
    /// </summary>
    /// <param name="value">The value to write</param>
    /// <returns>The same IWritableByteBuffer instance to allow fluent syntax</returns>
    IWritableByteBuffer WriteBytes( IByteBuffer value );

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
