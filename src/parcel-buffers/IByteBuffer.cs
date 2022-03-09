namespace Parcel.Buffers;

/// <summary>
/// A buffer handling interface
/// </summary>
public interface IByteBuffer
{
    /// <summary>
    /// Gets if the buffer can be read
    /// </summary>
    bool IsReadable { get; }

    /// <summary>
    /// Gets if the buffer can be written
    /// </summary>
    bool IsWritable { get; }

    /// <summary>
    /// Gets the length of the buffer
    /// </summary>
    int Length { get; }

    /// <summary>
    /// Gets the amount of readable bytes ahead of the current offset
    /// </summary>
    int ReadableBytes { get; }

    /// <summary>
    /// Discards all bytes in the buffer
    /// </summary>
    void DiscardAll();

    /// <summary>
    /// Discards read bytes from the buffer
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    void DiscardReadBytes();

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
    /// Gets a range of bytes as an IByteBuffer at the given offset without moving the buffer index
    /// </summary>
    /// <param name="offset">The offset to start reading the value</param>
    /// <param name="length">The number of bytes to read</param>
    /// <returns>A readable IByteBuffer value</returns>
    IByteBuffer GetByteBuffer( int offset, int length );

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
    /// Reads a boolean value
    /// </summary>
    /// <returns>A boolean value</returns>
    byte ReadByte();

    /// <summary>
    /// Reads a range of bytes
    /// </summary>
    /// <param name="length">The number of bytes to read</param>
    /// <returns>A byte[] value</returns>
    byte[] ReadBytes( int length );

    /// <summary>
    /// Reads a range of bytes as an IByteBuffer
    /// </summary>
    /// <param name="length">The number of bytes to read</param>
    /// <returns>A readable IByteBuffer value</returns>
    IByteBuffer ReadByteBuffer( int length );

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
    void ResetOffset();

    /// <summary>
    /// Reads and skips a range of bytes
    /// </summary>
    /// <param name="length">The number of bytes to skip</param>
    void SkipBytes( int length );

    /// <summary>
    /// Gets the entire buffer as a byte[] no matter where the reading/writing offset is
    /// </summary>
    /// <returns>A byte[] value</returns>
    byte[] ToArray();

    /// <summary>
    /// Writes a boolean value
    /// </summary>
    /// <param name="value">The value to write</param>
    void WriteBoolean( bool value );

    /// <summary>
    /// Writes a byte value
    /// </summary>
    /// <param name="value">The value to write</param>
    void WriteByte( byte value );

    /// <summary>
    /// Writes a range of bytes
    /// </summary>
    /// <param name="value">The value to write</param>
    /// <param name="startIndex">The index in the value to start writing from</param>
    /// <param name="length">The number of bytes to write</param>
    void WriteBytes( byte[] value, int startIndex, int length );

    /// <summary>
    /// Writes the contents of an IByteBuffer instance
    /// </summary>
    /// <param name="value">The value to write</param>
    void WriteByteBuffer( IByteBuffer value );

    /// <summary>
    /// Writes a double value
    /// </summary>
    /// <param name="value">The value to write</param>
    void WriteDouble( double value );

    /// <summary>
    /// Writes a float value
    /// </summary>
    /// <param name="value">The value to write</param>
    void WriteSingle( float value );

    /// <summary>
    /// Writes a short value
    /// </summary>
    /// <param name="value">The value to write</param>
    void WriteInt16( short value );

    /// <summary>
    /// Writes an int value
    /// </summary>
    /// <param name="value">The value to write</param>
    void WriteInt32( int value );

    /// <summary>
    /// Writes a long value
    /// </summary>
    /// <param name="value">The value to write</param>
    void WriteInt64( long value );

    /// <summary>
    /// Writes an ushort value
    /// </summary>
    /// <param name="value">The value to write</param>
    void WriteUInt16( ushort value );

    /// <summary>
    /// Writes an uint value
    /// </summary>
    /// <param name="value">The value to write</param>
    void WriteUInt32( uint value );

    /// <summary>
    /// Writes an ulong value
    /// </summary>
    /// <param name="value">The value to write</param>
    void WriteUInt64( ulong value );
}
