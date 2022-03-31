using System.Buffers.Binary;

namespace Faactory.Channels.Buffers;

/// <summary>
/// A writable IByteBuffer
/// </summary>
public sealed class WritableByteBuffer : IByteBuffer
{
    private readonly Endianness endianness;
    private readonly List<byte> buffer;

    public WritableByteBuffer( Endianness endianness = Endianness.BigEndian )
    {
        buffer = new List<byte>();
        this.endianness = endianness;
    }

    public WritableByteBuffer( byte[] source, Endianness endianness = Endianness.BigEndian )
    {
        buffer = new List<byte>( source );
        this.endianness = endianness;
    }

    public WritableByteBuffer( int capacity, Endianness endianness = Endianness.BigEndian )
    {
        buffer = new List<byte>( capacity );
        this.endianness = endianness;
    }

    public bool IsReadable => false;
    public bool IsWritable => true;
    public int Length => buffer.Count;
    public int ReadableBytes
        => throw new InvalidOperationException( "Invalid operation over a non-readable IByteBuffer." );

    public int Offset
        => throw new InvalidOperationException( "Invalid operation over a non-readable IByteBuffer." );

    public IByteBuffer DiscardAll()
    {
        buffer.Clear();

        return ( this );
    }

    public IByteBuffer DiscardReadBytes()
        => throw new InvalidOperationException( "Invalid operation over a non-readable IByteBuffer." );

    public bool GetBoolean( int offset )
        => throw new InvalidOperationException( "Invalid operation over a non-readable IByteBuffer." );

    public byte GetByte( int offset )
        => throw new InvalidOperationException( "Invalid operation over a non-readable IByteBuffer." );

    public byte[] GetBytes( int offset, int length )
        => throw new InvalidOperationException( "Invalid operation over a non-readable IByteBuffer." );        

    public IByteBuffer GetByteBuffer( int offset, int length )
        => throw new InvalidOperationException( "Invalid operation over a non-readable IByteBuffer." );        

    public double GetDouble( int offset )
        => throw new InvalidOperationException( "Invalid operation over a non-readable IByteBuffer." );        

    public float GetSingle( int offset )
        => throw new InvalidOperationException( "Invalid operation over a non-readable IByteBuffer." );        

    public short GetInt16( int offset )
        => throw new InvalidOperationException( "Invalid operation over a non-readable IByteBuffer." );        

    public int GetInt32( int offset )
        => throw new InvalidOperationException( "Invalid operation over a non-readable IByteBuffer." );        

    public long GetInt64( int offset )
        => throw new InvalidOperationException( "Invalid operation over a non-readable IByteBuffer." );        

    public ushort GetUInt16( int offset )
        => throw new InvalidOperationException( "Invalid operation over a non-readable IByteBuffer." );        

    public uint GetUInt32( int offset )
        => throw new InvalidOperationException( "Invalid operation over a non-readable IByteBuffer." );        

    public ulong GetUInt64( int offset )
        => throw new InvalidOperationException( "Invalid operation over a non-readable IByteBuffer." );        

    private T Read<T>( Func<int,T> read, int size )
        => throw new InvalidOperationException( "Invalid operation over a non-readable IByteBuffer." );        

    public bool ReadBoolean()
        => throw new InvalidOperationException( "Invalid operation over a non-readable IByteBuffer." );        

    public byte ReadByte()
        => throw new InvalidOperationException( "Invalid operation over a non-readable IByteBuffer." );        

    public byte[] ReadBytes( int length )
        => throw new InvalidOperationException( "Invalid operation over a non-readable IByteBuffer." );        

    public IByteBuffer ReadByteBuffer( int length )
        => throw new InvalidOperationException( "Invalid operation over a non-readable IByteBuffer." );        

    public double ReadDouble()
        => throw new InvalidOperationException( "Invalid operation over a non-readable IByteBuffer." );        

    public float ReadSingle()
        => throw new InvalidOperationException( "Invalid operation over a non-readable IByteBuffer." );        

    public short ReadInt16()
        => throw new InvalidOperationException( "Invalid operation over a non-readable IByteBuffer." );        

    public int ReadInt32()
        => throw new InvalidOperationException( "Invalid operation over a non-readable IByteBuffer." );        

    public long ReadInt64()
        => throw new InvalidOperationException( "Invalid operation over a non-readable IByteBuffer." );        

    public ushort ReadUInt16()
        => throw new InvalidOperationException( "Invalid operation over a non-readable IByteBuffer." );        

    public uint ReadUInt32()
        => throw new InvalidOperationException( "Invalid operation over a non-readable IByteBuffer." );        

    public ulong ReadUInt64()
        => throw new InvalidOperationException( "Invalid operation over a non-readable IByteBuffer." );

    public IByteBuffer ResetOffset()
        => throw new InvalidOperationException( "Invalid operation over a non-readable IByteBuffer." );

    public IByteBuffer SkipBytes( int length )
        => throw new InvalidOperationException( "Invalid operation over a non-readable IByteBuffer." );

    public byte[] ToArray() => buffer.ToArray();

    public IByteBuffer WriteBoolean( bool value )
    {
        var bytes = BitConverter.GetBytes( value );

        return WriteBytes( bytes, 0, bytes.Length );
    }

    public IByteBuffer WriteByte( byte value )
    {
        buffer.Add( value );

        return ( this );
    }

    public IByteBuffer WriteBytes( byte[] value, int startIndex, int length )
    {
        var bytes = value.Skip( startIndex )
            .Take( length );

        buffer.AddRange( bytes );

        return ( this );
    }

    public IByteBuffer WriteByteBuffer( IByteBuffer value )
    {
        var bytes = value.ToArray();

        return WriteBytes( bytes, 0, bytes.Length );
    }

    public IByteBuffer WriteDouble( double value )
    {
        var span = new Span<byte>( new byte[ sizeof( double ) ]);
        if ( endianness == Endianness.BigEndian )
        {
            BinaryPrimitives.WriteDoubleBigEndian( span, value );
        }
        else
        {
            BinaryPrimitives.WriteDoubleLittleEndian( span, value );
        }

        return WriteBytes( span.ToArray(), 0, span.Length );
    }

    public IByteBuffer WriteSingle( float value )
    {
        var span = new Span<byte>( new byte[ sizeof( float ) ]);
        if ( endianness == Endianness.BigEndian )
        {
            BinaryPrimitives.WriteSingleBigEndian( span, value );
        }
        else
        {
            BinaryPrimitives.WriteSingleLittleEndian( span, value );
        }

        return WriteBytes( span.ToArray(), 0, span.Length );
    }

    public IByteBuffer WriteInt16( Int16 value )
    {
        var span = new Span<byte>( new byte[ sizeof( Int16 ) ]);
        if ( endianness == Endianness.BigEndian )
        {
            BinaryPrimitives.WriteInt16BigEndian( span, value );
        }
        else
        {
            BinaryPrimitives.WriteInt16LittleEndian( span, value );
        }

        return WriteBytes( span.ToArray(), 0, span.Length );
    }

    public IByteBuffer WriteInt32( Int32 value )
    {
        var span = new Span<byte>( new byte[ sizeof( Int32 ) ]);
        if ( endianness == Endianness.BigEndian )
        {
            BinaryPrimitives.WriteInt32BigEndian( span, value );
        }
        else
        {
            BinaryPrimitives.WriteInt32LittleEndian( span, value );
        }

        return WriteBytes( span.ToArray(), 0, span.Length );
    }

    public IByteBuffer WriteInt64( Int64 value )
    {
        var span = new Span<byte>( new byte[ sizeof( Int64 ) ]);
        if ( endianness == Endianness.BigEndian )
        {
            BinaryPrimitives.WriteInt64BigEndian( span, value );
        }
        else
        {
            BinaryPrimitives.WriteInt64LittleEndian( span, value );
        }

        return WriteBytes( span.ToArray(), 0, span.Length );
    }

    public IByteBuffer WriteUInt16( UInt16 value )
    {
        var span = new Span<byte>( new byte[ sizeof( UInt16 ) ]);
        if ( endianness == Endianness.BigEndian )
        {
            BinaryPrimitives.WriteUInt16BigEndian( span, value );
        }
        else
        {
            BinaryPrimitives.WriteUInt16LittleEndian( span, value );
        }

        return WriteBytes( span.ToArray(), 0, span.Length );
    }

    public IByteBuffer WriteUInt32( UInt32 value )
    {
        var span = new Span<byte>( new byte[ sizeof( UInt32 ) ]);
        if ( endianness == Endianness.BigEndian )
        {
            BinaryPrimitives.WriteUInt32BigEndian( span, value );
        }
        else
        {
            BinaryPrimitives.WriteUInt32LittleEndian( span, value );
        }

        return WriteBytes( span.ToArray(), 0, span.Length );
    }

    public IByteBuffer WriteUInt64( UInt64 value )
    {
        var span = new Span<byte>( new byte[ sizeof( UInt64 ) ]);
        if ( endianness == Endianness.BigEndian )
        {
            BinaryPrimitives.WriteUInt64BigEndian( span, value );
        }
        else
        {
            BinaryPrimitives.WriteUInt64LittleEndian( span, value );
        }

        return WriteBytes( span.ToArray(), 0, span.Length );
    }
}
