using System.Buffers.Binary;

namespace Faactory.Channels.Buffers;

/// <summary>
/// A read-only ByteBuffer wrapping a byte[]
/// </summary>
public sealed class WrappedByteBuffer : IByteBuffer
{
    private readonly Endianness endianness;
    private byte[] buffer;
    private int offset;

    public WrappedByteBuffer( byte[] source, Endianness endianness = Endianness.BigEndian )
    {
        buffer = source;
        offset = 0;
        this.endianness = endianness;
    }

    public bool IsReadable => true;
    public bool IsWritable => false;
    public int Length => buffer.Length;
    public int ReadableBytes => Length - offset;

    public void DiscardAll()
    {
        Array.Resize<byte>( ref buffer, 0 );
    }

    public void DiscardReadBytes()
    {
        if ( offset <= 0 )
        {
            return;
        }

        var dest = new byte[buffer.Length - offset];
        Array.Copy( buffer, offset, dest, 0, buffer.Length - offset );

        buffer = dest;
    }

    public bool GetBoolean( int offset )
    {
        return BitConverter.ToBoolean( buffer, offset );
    }

    public byte GetByte( int offset )
    {
        return buffer[offset];
    }

    public byte[] GetBytes( int offset, int length )
    {
        var dest = new byte[length];

        Array.Copy( buffer, offset, dest, 0, length );

        return ( dest );
    }

    public IByteBuffer GetByteBuffer( int offset, int length )
    {
        var dest = GetBytes( offset, length );

        return new WrappedByteBuffer( dest );
    }

    public double GetDouble( int offset )
    {
        var span = new ReadOnlySpan<byte>( buffer, offset, sizeof( double ) );
        var value = ( endianness == Endianness.BigEndian )
            ? BinaryPrimitives.ReadDoubleBigEndian( span )
            : BinaryPrimitives.ReadDoubleLittleEndian( span );

        return value;
    }

    public float GetSingle( int offset )
    {
        var span = new ReadOnlySpan<byte>( buffer, offset, sizeof( float ) );
        var value = ( endianness == Endianness.BigEndian )
            ? BinaryPrimitives.ReadSingleBigEndian( span )
            : BinaryPrimitives.ReadSingleLittleEndian( span );

        return value;
    }

    public short GetInt16( int offset )
    {
        var span = new ReadOnlySpan<byte>( buffer, offset, sizeof( Int16 ) );
        var value = ( endianness == Endianness.BigEndian )
            ? BinaryPrimitives.ReadInt16BigEndian( span )
            : BinaryPrimitives.ReadInt16LittleEndian( span );

        return value;
    }

    public int GetInt32( int offset )
    {
        var span = new ReadOnlySpan<byte>( buffer, offset, sizeof( Int32 ) );
        var value = ( endianness == Endianness.BigEndian )
            ? BinaryPrimitives.ReadInt32BigEndian( span )
            : BinaryPrimitives.ReadInt32LittleEndian( span );

        return value;
    }

    public long GetInt64( int offset )
    {
        var span = new ReadOnlySpan<byte>( buffer, offset, sizeof( Int64 ) );
        var value = ( endianness == Endianness.BigEndian )
            ? BinaryPrimitives.ReadInt64BigEndian( span )
            : BinaryPrimitives.ReadInt64LittleEndian( span );

        return value;
    }

    public ushort GetUInt16( int offset )
    {
        var span = new ReadOnlySpan<byte>( buffer, offset, sizeof( UInt16 ) );
        var value = ( endianness == Endianness.BigEndian )
            ? BinaryPrimitives.ReadUInt16BigEndian( span )
            : BinaryPrimitives.ReadUInt16LittleEndian( span );

        return value;
    }

    public uint GetUInt32( int offset )
    {
        var span = new ReadOnlySpan<byte>( buffer, offset, sizeof( UInt32 ) );
        var value = ( endianness == Endianness.BigEndian )
            ? BinaryPrimitives.ReadUInt32BigEndian( span )
            : BinaryPrimitives.ReadUInt32LittleEndian( span );

        return value;
    }

    public ulong GetUInt64( int offset )
    {
        var span = new ReadOnlySpan<byte>( buffer, offset, sizeof( UInt64 ) );
        var value = ( endianness == Endianness.BigEndian )
            ? BinaryPrimitives.ReadUInt64BigEndian( span )
            : BinaryPrimitives.ReadUInt64LittleEndian( span );

        return value;
    }

    private T Read<T>( Func<int,T> read, int size )
    {
        var value = read( offset );

        offset += size;

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
        var value = GetBytes( offset, length );

        offset += length;

        return ( value );
    }

    public IByteBuffer ReadByteBuffer( int length )
    {
        var value = ReadBytes( length );

        return new WrappedByteBuffer( value );
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

    public void ResetOffset()
    {
        offset = 0;
    }

    public void SkipBytes( int length )
    {
        if ( ( offset + length ) > buffer.Length )
        {
            throw new ArgumentOutOfRangeException( nameof( length ) );
        }

        offset += length;
    }

    public byte[] ToArray() => buffer;

    public void WriteBoolean( bool value )
        => throw new InvalidOperationException( "Invalid operation over a non-writable IByteBuffer." );

    public void WriteByte( byte value )
        => throw new InvalidOperationException( "Invalid operation over a non-writable IByteBuffer." );

    public void WriteBytes( byte[] value, int startIndex, int length )
        => throw new InvalidOperationException( "Invalid operation over a non-writable IByteBuffer." );

    public void WriteByteBuffer( IByteBuffer value )
        => throw new InvalidOperationException( "Invalid operation over a non-writable IByteBuffer." );

    public void WriteDouble( double value )
        => throw new InvalidOperationException( "Invalid operation over a non-writable IByteBuffer." );

    public void WriteSingle( float value )
        => throw new InvalidOperationException( "Invalid operation over a non-writable IByteBuffer." );

    public void WriteInt16( Int16 value )
        => throw new InvalidOperationException( "Invalid operation over a non-writable IByteBuffer." );

    public void WriteInt32( Int32 value )
        => throw new InvalidOperationException( "Invalid operation over a non-writable IByteBuffer." );

    public void WriteInt64( Int64 value )
        => throw new InvalidOperationException( "Invalid operation over a non-writable IByteBuffer." );

    public void WriteUInt16( UInt16 value )
        => throw new InvalidOperationException( "Invalid operation over a non-writable IByteBuffer." );

    public void WriteUInt32( UInt32 value )
        => throw new InvalidOperationException( "Invalid operation over a non-writable IByteBuffer." );

    public void WriteUInt64( UInt64 value )
        => throw new InvalidOperationException( "Invalid operation over a non-writable IByteBuffer." );
}
