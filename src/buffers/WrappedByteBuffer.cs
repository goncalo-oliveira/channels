using System.Buffers.Binary;

namespace Faactory.Channels.Buffers;

/// <summary>
/// A read-only ByteBuffer wrapping a byte[]
/// </summary>
public sealed class WrappedByteBuffer( byte[] source, Endianness endianness = Endianness.BigEndian ) : IByteBuffer
{
    private byte[] buffer = source;

    /// <summary>
    /// Creates a new read-only buffer instance from the given base64 encoded string
    /// </summary>
    /// <param name="value">The base64 encoded string</param>
    /// <returns>A new read-only buffer instance</returns>
    public static IByteBuffer FromBase64String( string value )
        => new WrappedByteBuffer( Convert.FromBase64String( value ) );

    /// <summary>
    /// Creates a new read-only buffer instance from the given base64 encoded string
    /// </summary>
    /// <param name="value">The base64 encoded string</param>
    /// <param name="endianness">The endianness of the buffer</param>
    /// <returns>A new read-only buffer instance</returns>
    public static IByteBuffer FromBase64String( string value, Endianness endianness )
        => new WrappedByteBuffer( Convert.FromBase64String( value ), endianness );

    /// <summary>
    /// Creates a new read-only buffer instance from the given hex string
    /// </summary>
    /// <param name="value">The hex string</param>
    /// <returns>A new read-only buffer instance</returns>
    public static IByteBuffer FromHexString( string value )
        => new WrappedByteBuffer( Convert.FromHexString( value ) );

    /// <summary>
    /// Creates a new read-only buffer instance from the given hex string
    /// </summary>
    /// <param name="value">The hex string</param>
    /// <param name="endianness">The endianness of the buffer</param>
    /// <returns>A new read-only buffer instance</returns>
    public static IByteBuffer FromHexString( string value, Endianness endianness )
        => new WrappedByteBuffer( Convert.FromHexString( value ), endianness );

    public Endianness Endianness { get; } = endianness;
    public bool IsReadable => true;
    public bool IsWritable => false;
    public int Length => buffer.Length;
    public int ReadableBytes => Length - Offset;

    public int Offset { get; private set; } = 0;

    public IByteBuffer DiscardAll()
    {
        Array.Resize( ref buffer, 0 );

        ResetOffset();

        return this;
    }

    public IByteBuffer DiscardReadBytes()
    {
        if ( Offset <= 0 )
        {
            return this;
        }

        var dest = new byte[buffer.Length - Offset];
        Array.Copy( buffer, Offset, dest, 0, buffer.Length - Offset );

        buffer = dest;

        ResetOffset();

        return this;
    }

    public bool GetBoolean( int offset ) => BitConverter.ToBoolean( buffer, offset );

    public byte GetByte( int offset ) => buffer[offset];

    public byte[] GetBytes( int offset, int length )
    {
        var dest = new byte[length];

        Array.Copy( buffer, offset, dest, 0, length );

        return dest;
    }

    public IByteBuffer GetByteBuffer( int offset, int length )
    {
        var dest = GetBytes( offset, length );

        return new WrappedByteBuffer( dest, Endianness );
    }

    public double GetDouble( int offset )
    {
        var span = new ReadOnlySpan<byte>( buffer, offset, sizeof( double ) );
        var value = ( Endianness == Endianness.BigEndian )
            ? BinaryPrimitives.ReadDoubleBigEndian( span )
            : BinaryPrimitives.ReadDoubleLittleEndian( span );

        return value;
    }

    public float GetSingle( int offset )
    {
        var span = new ReadOnlySpan<byte>( buffer, offset, sizeof( float ) );
        var value = ( Endianness == Endianness.BigEndian )
            ? BinaryPrimitives.ReadSingleBigEndian( span )
            : BinaryPrimitives.ReadSingleLittleEndian( span );

        return value;
    }

    public short GetInt16( int offset )
    {
        var span = new ReadOnlySpan<byte>( buffer, offset, sizeof( Int16 ) );
        var value = ( Endianness == Endianness.BigEndian )
            ? BinaryPrimitives.ReadInt16BigEndian( span )
            : BinaryPrimitives.ReadInt16LittleEndian( span );

        return value;
    }

    public int GetInt32( int offset )
    {
        var span = new ReadOnlySpan<byte>( buffer, offset, sizeof( Int32 ) );
        var value = ( Endianness == Endianness.BigEndian )
            ? BinaryPrimitives.ReadInt32BigEndian( span )
            : BinaryPrimitives.ReadInt32LittleEndian( span );

        return value;
    }

    public long GetInt64( int offset )
    {
        var span = new ReadOnlySpan<byte>( buffer, offset, sizeof( Int64 ) );
        var value = ( Endianness == Endianness.BigEndian )
            ? BinaryPrimitives.ReadInt64BigEndian( span )
            : BinaryPrimitives.ReadInt64LittleEndian( span );

        return value;
    }

    public ushort GetUInt16( int offset )
    {
        var span = new ReadOnlySpan<byte>( buffer, offset, sizeof( UInt16 ) );
        var value = ( Endianness == Endianness.BigEndian )
            ? BinaryPrimitives.ReadUInt16BigEndian( span )
            : BinaryPrimitives.ReadUInt16LittleEndian( span );

        return value;
    }

    public uint GetUInt32( int offset )
    {
        var span = new ReadOnlySpan<byte>( buffer, offset, sizeof( UInt32 ) );
        var value = ( Endianness == Endianness.BigEndian )
            ? BinaryPrimitives.ReadUInt32BigEndian( span )
            : BinaryPrimitives.ReadUInt32LittleEndian( span );

        return value;
    }

    public ulong GetUInt64( int offset )
    {
        var span = new ReadOnlySpan<byte>( buffer, offset, sizeof( UInt64 ) );
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

    public bool ReadBoolean()
    {
        return Read( GetBoolean, sizeof( byte ) );
    }

    public byte ReadByte()
    {
        return Read( GetByte, sizeof( byte ) );
    }

    public byte[] ReadBytes( int length )
    {
        var value = GetBytes( Offset, length );

        Offset += length;

        return ( value );
    }

    public IByteBuffer ReadByteBuffer( int length )
    {
        var value = ReadBytes( length );

        return new WrappedByteBuffer( value, Endianness );
    }

    public double ReadDouble()
    {
        return Read( GetDouble, sizeof( double ) );
    }

    public float ReadSingle()
    {
        return Read( GetSingle, sizeof( float ) );
    }

    public short ReadInt16()
    {
        return Read( GetInt16, sizeof( Int16 ) );
    }

    public int ReadInt32()
    {
        return Read( GetInt32, sizeof( Int32 ) );
    }

    public long ReadInt64()
    {
        return Read( GetInt64, sizeof( Int64 ) );
    }

    public ushort ReadUInt16()
    {
        return Read( GetUInt16, sizeof( UInt16 ) );
    }

    public uint ReadUInt32()
    {
        return Read( GetUInt32, sizeof( UInt32 ) );
    }

    public ulong ReadUInt64()
    {
        return Read( GetUInt64, sizeof( UInt64 ) );
    }

    public IByteBuffer ResetOffset()
    {
        Offset = 0;

        return ( this );
    }

    public IByteBuffer SkipBytes( int length )
    {
        if ( ( Offset + length ) > buffer.Length )
        {
            throw new ArgumentOutOfRangeException( nameof( length ) );
        }

        Offset += length;

        return ( this );
    }

    public byte[] ToArray() => buffer;

    public IByteBuffer UndoRead( int length )
    {
        if ( ( Offset - length ) < 0 )
        {
            throw new ArgumentOutOfRangeException( nameof( length ) );
        }

        Offset -= length;

        return this;
    }

    public IByteBuffer WriteBoolean( bool value )
        => throw new NonWritableBufferException();

    public IByteBuffer WriteByte( byte value )
        => throw new NonWritableBufferException();

    public IByteBuffer WriteBytes( byte[] value, int startIndex, int length )
        => throw new NonWritableBufferException();

    public IByteBuffer WriteByteBuffer( IByteBuffer value )
        => throw new NonWritableBufferException();

    public IByteBuffer WriteDouble( double value )
        => throw new NonWritableBufferException();

    public IByteBuffer WriteSingle( float value )
        => throw new NonWritableBufferException();

    public IByteBuffer WriteInt16( Int16 value )
        => throw new NonWritableBufferException();

    public IByteBuffer WriteInt32( Int32 value )
        => throw new NonWritableBufferException();

    public IByteBuffer WriteInt64( Int64 value )
        => throw new NonWritableBufferException();

    public IByteBuffer WriteUInt16( UInt16 value )
        => throw new NonWritableBufferException();

    public IByteBuffer WriteUInt32( UInt32 value )
        => throw new NonWritableBufferException();

    public IByteBuffer WriteUInt64( UInt64 value )
        => throw new NonWritableBufferException();
}
