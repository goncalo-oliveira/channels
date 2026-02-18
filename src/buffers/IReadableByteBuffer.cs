namespace Faactory.Channels.Buffers;

/// <summary>
/// A buffer handling interface with read-only capabilities
/// </summary>
[Serialization.ByteBufferJsonConverter]
public interface IReadableByteBuffer : IByteBuffer
{
    /// <summary>
    /// Gets the amount of readable bytes ahead of the current offset
    /// </summary>
    int ReadableBytes { get; }

    /// <summary>
    /// Gets the the current offset
    /// </summary>
    int Offset { get; }

    /// <summary>
    /// Discards all bytes in the buffer
    /// </summary>
    /// <returns>The same IReadableByteBuffer instance to allow fluent syntax</returns>
    IReadableByteBuffer DiscardAll();

    /// <summary>
    /// Discards read bytes from the buffer
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    /// <returns>The same IReadableByteBuffer instance to allow fluent syntax</returns>
    IReadableByteBuffer DiscardReadBytes();

    /// <summary>
    /// Gets a boolean value from the byte at the given offset without moving the buffer index
    /// </summary>
    /// <param name="offset">The offset to start reading the value</param>
    /// <returns>A boolean value</returns>
    bool GetBoolean( int offset );

    /// <summary>
    /// Gets the byte value at the given offset without moving the buffer index
    /// </summary>
    /// <param name="offset">The offset to start reading the value</param>
    /// <returns>A byte value</returns>
    byte GetByte( int offset );

    /// <summary>
    /// Gets a range of bytes at the given offset without moving the buffer index
    /// </summary>
    /// <param name="offset">The offset to start reading the value</param>
    /// <param name="length">The number of bytes to read</param>
    /// <returns>A byte[] value</returns>
    byte[] GetBytes( int offset, int length );

    /// <summary>
    /// Gets a range of bytes at the given offset without moving the buffer index as <see cref="ReadOnlySpan{T}"/>
    /// </summary>
    /// <param name="offset">The offset to start reading the value</param>
    /// <param name="length">The number of bytes to read</param>
    /// <returns>A readonly span value</returns>
    ReadOnlySpan<byte> GetSpan( int offset, int length );

    /// <summary>
    /// Gets a range of bytes as an IReadableByteBuffer at the given offset without moving the buffer index
    /// </summary>
    /// <param name="offset">The offset to start reading the value</param>
    /// <param name="length">The number of bytes to read</param>
    /// <returns>A IReadableByteBuffer value</returns>
    IReadableByteBuffer GetByteBuffer( int offset, int length );

    /// <summary>
    /// Gets a double value at the given offset without moving the buffer index
    /// </summary>
    /// <param name="offset">The offset to start reading the value</param>
    /// <returns>A double value</returns>
    double GetDouble( int offset );

    /// <summary>
    /// Gets a float value at the given offset without moving the buffer index
    /// </summary>
    /// <param name="offset">The offset to start reading the value</param>
    /// <returns>A float value</returns>
    float GetSingle( int offset );

    /// <summary>
    /// Gets a short value at the given offset without moving the buffer index
    /// </summary>
    /// <param name="offset">The offset to start reading the value</param>
    /// <returns>A short value</returns>
    short GetInt16( int offset );

    /// <summary>
    /// Gets an int value at the given offset without moving the buffer index
    /// </summary>
    /// <param name="offset">The offset to start reading the value</param>
    /// <returns>An int value</returns>
    int GetInt32( int offset );

    /// <summary>
    /// Gets a long value at the given offset without moving the buffer index
    /// </summary>
    /// <param name="offset">The offset to start reading the value</param>
    /// <returns>A long value</returns>
    long GetInt64( int offset );

    /// <summary>
    /// Gets an ushort value at the given offset without moving the buffer index
    /// </summary>
    /// <param name="offset">The offset to start reading the value</param>
    /// <returns>An ushort value</returns>
    ushort GetUInt16( int offset );

    /// <summary>
    /// Gets an uint value at the given offset without moving the buffer index
    /// </summary>
    /// <param name="offset">The offset to start reading the value</param>
    /// <returns>An uint value</returns>
    uint GetUInt32( int offset );

    /// <summary>
    /// Gets an ulong value at the given offset without moving the buffer index
    /// </summary>
    /// <param name="offset">The offset to start reading the value</param>
    /// <returns>An ulong value</returns>
    ulong GetUInt64( int offset );

    /// <summary>
    /// Reads a boolean value
    /// </summary>
    /// <returns>A boolean value</returns>
    bool ReadBoolean();

    /// <summary>
    /// Reads a byte value
    /// </summary>
    /// <returns>A byte value</returns>
    byte ReadByte();

    /// <summary>
    /// Reads a range of bytes
    /// </summary>
    /// <param name="length">The number of bytes to read</param>
    /// <returns>A byte[] value</returns>
    byte[] ReadBytes( int length );

    /// <summary>
    /// Reads a range of bytes as <see cref="ReadOnlySpan{T}"/>
    /// </summary>
    /// <param name="length">The number of bytes to read</param>
    /// <returns>A readonly span value</returns>
    ReadOnlySpan<byte> ReadSpan( int length );

    /// <summary>
    /// Reads a range of bytes as a IReadableByteBuffer
    /// </summary>
    /// <param name="length">The number of bytes to read</param>
    /// <returns>A IReadableByteBuffer value</returns>
    IReadableByteBuffer ReadByteBuffer( int length );

    /// <summary>
    /// Reads a double value
    /// </summary>
    /// <returns>A double value</returns>
    double ReadDouble();

    /// <summary>
    /// Reads a float value
    /// </summary>
    /// <returns>A float value</returns>
    float ReadSingle();

    /// <summary>
    /// Reads a short value
    /// </summary>
    /// <returns>A short value</returns>
    short ReadInt16();

    /// <summary>
    /// Reads an int value
    /// </summary>
    /// <returns>An int value</returns>
    int ReadInt32();

    /// <summary>
    /// Reads a long value
    /// </summary>
    /// <returns>A long value</returns>
    long ReadInt64();

    /// <summary>
    /// Reads an ushort value
    /// </summary>
    /// <returns>An ushort value</returns>
    ushort ReadUInt16();

    /// <summary>
    /// Reads an uint value
    /// </summary>
    /// <returns>An uint value</returns>
    uint ReadUInt32();

    /// <summary>
    /// Reads an ulong value
    /// </summary>
    /// <returns>An ulong value</returns>
    ulong ReadUInt64();

    /// <summary>
    /// Resets the reading offset
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    /// <returns>The same IReadableByteBuffer instance to allow fluent syntax</returns>
    IReadableByteBuffer ResetOffset();

    /// <summary>
    /// Reads and skips a range of bytes
    /// </summary>
    /// <param name="length">The number of bytes to skip</param>
    /// <returns>The same IReadableByteBuffer instance to allow fluent syntax</returns>
    IReadableByteBuffer SkipBytes( int length );

    /// <summary>
    /// Moves the reading offset back by the given length
    /// </summary>
    /// <param name="length">The number of bytes to move back the reading offset</param>
    /// <returns>The same IReadableByteBuffer instance to allow fluent syntax</returns>
    IReadableByteBuffer UndoRead( int length );
}
