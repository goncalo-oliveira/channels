
using System.Buffers.Binary;

namespace Faactory.Channels.Buffers;

/// <summary>
/// A readable ByteBuffer
/// </summary>
/// <param name="buffer"></param>
/// <param name="endianness"></param>
public sealed class ReadableByteBuffer( byte[] buffer, Endianness endianness = Endianness.BigEndian ) : IReadableByteBuffer
{
    /// <summary>
    /// Creates a new readable buffer instance from the given base64 encoded string
    /// </summary>
    /// <param name="value">The base64 encoded string</param>
    /// <returns>A new readable buffer instance</returns>
    public static IReadableByteBuffer FromBase64String( string value )
        => new ReadableByteBuffer( Convert.FromBase64String( value ) );

    /// <summary>
    /// Creates a new readable buffer instance from the given base64 encoded string
    /// </summary>
    /// <param name="value">The base64 encoded string</param>
    /// <param name="endianness">The endianness of the buffer</param>
    /// <returns>A new readable buffer instance</returns>
    public static IReadableByteBuffer FromBase64String( string value, Endianness endianness )
        => new ReadableByteBuffer( Convert.FromBase64String( value ), endianness );

    /// <summary>
    /// Creates a new readable buffer instance from the given hex string
    /// </summary>
    /// <param name="value">The hex string</param>
    /// <returns>A new readable buffer instance</returns>
    public static IReadableByteBuffer FromHexString( string value )
        => new ReadableByteBuffer( Convert.FromHexString( value ) );

    /// <summary>
    /// Creates a new readable buffer instance from the given hex string
    /// </summary>
    /// <param name="value">The hex string</param>
    /// <param name="endianness">The endianness of the buffer</param>
    /// <returns>A new readable buffer instance</returns>
    public static IReadableByteBuffer FromHexString( string value, Endianness endianness )
        => new ReadableByteBuffer( Convert.FromHexString( value ), endianness );

    /// <summary>
    /// Gets the number of readable bytes in the buffer
    /// </summary>
    public int ReadableBytes => Length - Offset;

    /// <summary>
    /// Gets the current offset in the buffer. This is the position from which the next read operation will occur.
    /// </summary>
    public int Offset { get; private set; } = 0;

    /// <summary>
    /// Gets the endianness of the buffer, which determines how multi-byte values are read from the buffer.
    /// </summary>
    public Endianness Endianness { get; } = endianness;

    /// <summary>
    /// Gets the total length of the buffer in bytes
    /// </summary>
    public int Length => buffer.Length;

    /// <summary>
    /// Discards all bytes in the buffer, effectively clearing it.
    /// </summary>
    /// <returns>The current buffer instance</returns>
    public IReadableByteBuffer DiscardAll()
    {
        Array.Resize( ref buffer, 0 );

        ResetOffset();

        return this;
    }

    /// <summary>
    /// Discards all bytes that have been read from the buffer, keeping only the unread bytes.
    /// </summary>
    /// <returns>The current buffer instance</returns>
    public IReadableByteBuffer DiscardReadBytes()
    {
        if ( Offset == 0 )
        {
            return this;
        }

        var remaining = Length - Offset;

        var dest = new byte[remaining];
        Array.Copy( buffer, Offset, dest, 0, remaining );

        buffer = dest;

        ResetOffset();

        return this;
    }

    private void ThrowIfOutOfRange( int offset, int length )
    {
        if ( offset < 0 || (uint)offset > (uint)Length )
        {
            throw new ArgumentOutOfRangeException( nameof( offset ) );
        }

        if ( (uint)length > (uint)( Length - offset ) )
        {
            throw new ArgumentOutOfRangeException( nameof( length ) );
        }
    }

    /// <summary>
    /// Gets a boolean value from the buffer at the specified offset.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the offset is out of range or there are not enough bytes to read a boolean value.</exception>
    public bool GetBoolean( int offset )
    {
        ThrowIfOutOfRange( offset, sizeof( byte ) );

        return BitConverter.ToBoolean( buffer, offset );
    }

    /// <summary>
    /// Gets a byte value from the buffer at the specified offset.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the offset is out of range or there are not enough bytes to read a byte value.</exception>
    public byte GetByte( int offset )
    {
        ThrowIfOutOfRange( offset, sizeof( byte ) );

        return buffer[offset];
    }

    /// <summary>
    /// Gets a byte array from the buffer at the specified offset and length.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the offset or length is out of range or there are not enough bytes to read the specified length.</exception>
    public byte[] GetBytes( int offset, int length )
    {
        ThrowIfOutOfRange( offset, length );

        var dest = new byte[length];

        Array.Copy( buffer, offset, dest, 0, length );

        return dest;
    }

    /// <summary>
    /// Gets a <see cref="ReadOnlySpan{T}"/> from the buffer at the specified offset and length.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the offset or length is out of range or there are not enough bytes to read the specified length.</exception>
    public ReadOnlySpan<byte> GetSpan( int offset, int length )
    {
        ThrowIfOutOfRange( offset, length );

        return new( buffer, offset, length );
    }

    /// <summary>
    /// Gets a new IReadableByteBuffer instance that wraps a portion of the buffer at the specified offset and length.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the offset or length is out of range or there are not enough bytes to read the specified length.</exception>
    public IReadableByteBuffer GetByteBuffer( int offset, int length )
    {
        var dest = GetBytes( offset, length );

        return new ReadableByteBuffer( dest, Endianness );
    }

    /// <summary>
    /// Gets a double value from the buffer at the specified offset, taking into account the endianness of the buffer.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the offset is out of range or there are not enough bytes to read a double value.</exception>
    public double GetDouble( int offset )
    {
        var span = GetSpan( offset, sizeof( double ) );
        var value = ( Endianness == Endianness.BigEndian )
            ? BinaryPrimitives.ReadDoubleBigEndian( span )
            : BinaryPrimitives.ReadDoubleLittleEndian( span );

        return value;
    }

    /// <summary>
    /// Gets a float value from the buffer at the specified offset, taking into account the endianness of the buffer.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the offset is out of range or there are not enough bytes to read a float value.</exception>
    public float GetSingle( int offset )
    {
        var span = GetSpan( offset, sizeof( float ) );
        var value = ( Endianness == Endianness.BigEndian )
            ? BinaryPrimitives.ReadSingleBigEndian( span )
            : BinaryPrimitives.ReadSingleLittleEndian( span );

        return value;
    }

    /// <summary>
    /// Gets a short value from the buffer at the specified offset, taking into account the endianness of the buffer.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the offset is out of range or there are not enough bytes to read a short value.</exception>
    public short GetInt16( int offset )
    {
        var span = GetSpan( offset, sizeof( short ) );
        var value = ( Endianness == Endianness.BigEndian )
            ? BinaryPrimitives.ReadInt16BigEndian( span )
            : BinaryPrimitives.ReadInt16LittleEndian( span );

        return value;
    }

    /// <summary>
    /// Gets an int value from the buffer at the specified offset, taking into account the endianness of the buffer.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the offset is out of range or there are not enough bytes to read an int value.</exception>
    public int GetInt32( int offset )
    {
        var span = GetSpan( offset, sizeof( int ) );
        var value = ( Endianness == Endianness.BigEndian )
            ? BinaryPrimitives.ReadInt32BigEndian( span )
            : BinaryPrimitives.ReadInt32LittleEndian( span );

        return value;
    }

    /// <summary>
    /// Gets a long value from the buffer at the specified offset, taking into account the endianness of the buffer.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the offset is out of range or there are not enough bytes to read a long value.</exception>
    public long GetInt64( int offset )
    {
        var span = GetSpan( offset, sizeof( long ) );
        var value = ( Endianness == Endianness.BigEndian )
            ? BinaryPrimitives.ReadInt64BigEndian( span )
            : BinaryPrimitives.ReadInt64LittleEndian( span );

        return value;
    }

    /// <summary>
    /// Gets an unsigned short value from the buffer at the specified offset, taking into account the endianness of the buffer.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the offset is out of range or there are not enough bytes to read an unsigned short value.</exception>
    public ushort GetUInt16( int offset )
    {
        var span = GetSpan( offset, sizeof( UInt16 ) );
        var value = ( Endianness == Endianness.BigEndian )
            ? BinaryPrimitives.ReadUInt16BigEndian( span )
            : BinaryPrimitives.ReadUInt16LittleEndian( span );

        return value;
    }

    /// <summary>
    /// Gets an unsigned int value from the buffer at the specified offset, taking into account the endianness of the buffer.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the offset is out of range or there are not enough bytes to read an unsigned int value.</exception>
    public uint GetUInt32( int offset )
    {
        var span = GetSpan( offset, sizeof( UInt32 ) );
        var value = ( Endianness == Endianness.BigEndian )
            ? BinaryPrimitives.ReadUInt32BigEndian( span )
            : BinaryPrimitives.ReadUInt32LittleEndian( span );

        return value;
    }

    /// <summary>
    /// Gets an unsigned long value from the buffer at the specified offset, taking into account the endianness of the buffer.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the offset is out of range or there are not enough bytes to read an unsigned long value.</exception>
    public ulong GetUInt64( int offset )
    {
        var span = GetSpan( offset, sizeof( UInt64 ) );
        var value = ( Endianness == Endianness.BigEndian )
            ? BinaryPrimitives.ReadUInt64BigEndian( span )
            : BinaryPrimitives.ReadUInt64LittleEndian( span );

        return value;
    }

    private T Read<T>( Func<int,T> read, int size )
    {
        var value = read( Offset );

        Offset += size;

        return ( value );
    }

    /// <summary>
    /// Reads a boolean value from the buffer at the current offset and advances the offset by the size of a boolean value.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when there are not enough bytes to read a boolean value at the current offset.</exception>
    public bool ReadBoolean()
        => Read( GetBoolean, sizeof( byte ) );

    /// <summary>
    /// Reads a byte value from the buffer at the current offset and advances the offset by the size of a byte value.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when there are not enough bytes to read a byte value at the current offset.</exception>
    public byte ReadByte()
        => Read( GetByte, sizeof( byte ) );

    /// <summary>
    /// Reads a sequence of bytes from the buffer at the current offset and advances the offset by the specified length.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when there are not enough bytes to read the specified length at the current offset.</exception>
    public byte[] ReadBytes( int length )
    {
        var value = GetBytes( Offset, length );

        Offset += length;

        return value;
    }

    /// <summary>
    /// Reads a span of bytes from the buffer at the current offset and advances the offset by the specified length.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when there are not enough bytes to read the specified length at the current offset.</exception>
    public ReadOnlySpan<byte> ReadSpan( int length )
    {
        var value = GetSpan( Offset, length );

        Offset += length;

        return value;
    }

    /// <summary>
    /// Reads a portion of the buffer as a new IReadableByteBuffer instance at the current offset and advances the offset by the specified length.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when there are not enough bytes to read the specified length at the current offset.</exception>
    public IReadableByteBuffer ReadByteBuffer( int length )
    {
        var bytes = ReadBytes( length );

        return new ReadableByteBuffer( bytes, Endianness );
    }

    /// <summary>
    /// Reads a double value from the buffer at the current offset, taking into account the endianness of the buffer, and advances the offset by the size of a double value.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when there are not enough bytes to read a double value at the current offset.</exception>
    public double ReadDouble()
        => Read( GetDouble, sizeof( double ) );

    /// <summary>
    /// Reads a float value from the buffer at the current offset, taking into account the endianness of the buffer, and advances the offset by the size of a float value.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when there are not enough bytes to read a float value at the current offset.</exception>
    public float ReadSingle()
        => Read( GetSingle, sizeof( float ) );

    /// <summary>
    /// Reads a short value from the buffer at the current offset, taking into account the endianness of the buffer, and advances the offset by the size of a short value.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when there are not enough bytes to read a short value at the current offset.</exception>
    public short ReadInt16()
        => Read( GetInt16, sizeof( short ) );

    /// <summary>
    /// Reads an int value from the buffer at the current offset, taking into account the endianness of the buffer, and advances the offset by the size of an int value.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when there are not enough bytes to read an int value at the current offset.</exception>
    public int ReadInt32()
        => Read( GetInt32, sizeof( int ) );

    /// <summary>
    /// Reads a long value from the buffer at the current offset, taking into account the endianness of the buffer, and advances the offset by the size of a long value.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when there are not enough bytes to read a long value at the current offset.</exception>
    public long ReadInt64()
        => Read( GetInt64, sizeof( long ) );

    /// <summary>
    /// Reads an unsigned short value from the buffer at the current offset, taking into account the endianness of the buffer, and advances the offset by the size of an unsigned short value.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when there are not enough bytes to read an unsigned short value at the current offset.</exception>
    public ushort ReadUInt16()
        => Read( GetUInt16, sizeof( ushort ) );

    /// <summary>
    /// Reads an unsigned int value from the buffer at the current offset, taking into account the endianness of the buffer, and advances the offset by the size of an unsigned int value.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when there are not enough bytes to read an unsigned int value at the current offset.</exception>
    public uint ReadUInt32()
        => Read( GetUInt32, sizeof( uint ) );

    /// <summary>
    /// Reads an unsigned long value from the buffer at the current offset, taking into account the endianness of the buffer, and advances the offset by the size of an unsigned long value.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when there are not enough bytes to read an unsigned long value at the current offset.</exception>
    public ulong ReadUInt64()
        => Read( GetUInt64, sizeof( ulong ) );

    /// <summary>
    /// Resets the offset to the beginning of the buffer, allowing for re-reading the buffer from the start.
    /// </summary>
    public IReadableByteBuffer ResetOffset()
    {
        Offset = 0;

        return this;
    }

    /// <summary>
    /// Skips a specified number of bytes in the buffer by advancing the offset, allowing for skipping over unwanted data.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when there are not enough bytes to skip the specified length at the current offset.</exception>
    public IReadableByteBuffer SkipBytes( int length )
    {
        ThrowIfOutOfRange( Offset, length );

        Offset += length;

        return this;
    }

    /// <summary>
    /// Returns the underlying buffer as a byte array.
    /// </summary>
    public byte[] ToArray()
        => buffer;

    /// <summary>
    /// Gets the underlying buffer as a <see cref="ReadOnlySpan{T}"/>
    /// </summary>
    /// <returns>A <see cref="ReadOnlySpan{T}"/> representing the used portion of the buffer</returns>
    public ReadOnlySpan<byte> AsSpan()
        => buffer.AsSpan();

    /// <summary>
    /// Undoes a read operation by moving the offset back by the specified length, allowing for re-reading previously read data.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when there are not enough bytes to undo the specified length at the current offset.</exception>
    public IReadableByteBuffer UndoRead( int length )
    {
        if ( ( Offset - length ) < 0 )
        {
            throw new ArgumentOutOfRangeException( nameof( length ) );
        }

        Offset -= length;

        return this;
    }
}
