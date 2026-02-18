namespace Faactory.Channels.Buffers;

/// <summary>
/// A buffer handling interface with write capabilities
/// </summary>
[Serialization.ByteBufferJsonConverter]
public interface IWritableByteBuffer : IByteBuffer
{
    /// <summary>
    /// Discards all written bytes and reallocates the buffer to its initial capacity.
    /// </summary>
    /// <returns>The same IWritableByteBuffer instance to allow fluent syntax</returns>
    IWritableByteBuffer Clear();

    /// <summary>
    /// Resets the writing offset to the beginning of the buffer, effectively discarding all written bytes. Current buffer capacity remains unchanged.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    /// <returns>The same IWritableByteBuffer instance to allow fluent syntax</returns>
    IWritableByteBuffer ResetOffset();

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
    /// Writes the contents of an IReadableByteBuffer instance
    /// </summary>
    /// <param name="value">The value to write</param>
    /// <returns>The same IWritableByteBuffer instance to allow fluent syntax</returns>
    IWritableByteBuffer WriteBytes( IReadableByteBuffer value );

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
