namespace Parcel.Buffers;

/// <summary>
/// A writable IByteBuffer
/// </summary>
public sealed class WritableByteBuffer : IByteBuffer
{
    private readonly List<byte> buffer;

    public WritableByteBuffer()
    {
        buffer = new List<byte>();
    }

    public WritableByteBuffer( byte[] source )
    {
        buffer = new List<byte>( source );
    }

    public WritableByteBuffer( int capacity )
    {
        buffer = new List<byte>( capacity );
    }

    public bool IsReadable => false;
    public bool IsWritable => true;
    public int Length => buffer.Count;
    public int ReadableBytes
        => throw new InvalidOperationException( "Invalid operation over a non-readable IByteBuffer." );

    public void DiscardAll()
    {
        buffer.Clear();
    }

    public void DiscardReadBytes()
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

    public void ResetOffset()
        => throw new InvalidOperationException( "Invalid operation over a non-readable IByteBuffer." );

    public void SkipBytes( int length )
        => throw new InvalidOperationException( "Invalid operation over a non-readable IByteBuffer." );

    public byte[] ToArray() => buffer.ToArray();

    public void WriteBoolean( bool value )
    {
        var bytes = BitConverter.GetBytes( value );

        WriteBytes( bytes, 0, bytes.Length );
    }

    public void WriteByte( byte value )
    {
        buffer.Add( value );
    }

    public void WriteBytes( byte[] value, int startIndex, int length )
    {
        var bytes = value.Skip( startIndex )
            .Take( length );

        buffer.AddRange( bytes );
    }

    public void WriteByteBuffer( IByteBuffer value )
    {
        var bytes = value.ToArray();

        WriteBytes( bytes, 0, bytes.Length );
    }

    public void WriteDouble( double value )
    {
        var bytes = BitConverter.GetBytes( value );

        WriteBytes( bytes, 0, bytes.Length );
    }

    public void WriteSingle( float value )
    {
        var bytes = BitConverter.GetBytes( value );

        WriteBytes( bytes, 0, bytes.Length );
    }

    public void WriteInt16( Int16 value )
    {
        var bytes = BitConverter.GetBytes( value );

        WriteBytes( bytes, 0, bytes.Length );
    }

    public void WriteInt32( Int32 value )
    {
        var bytes = BitConverter.GetBytes( value );

        WriteBytes( bytes, 0, bytes.Length );
    }

    public void WriteInt64( Int64 value )
    {
        var bytes = BitConverter.GetBytes( value );

        WriteBytes( bytes, 0, bytes.Length );
    }

    public void WriteUInt16( UInt16 value )
    {
        var bytes = BitConverter.GetBytes( value );

        WriteBytes( bytes, 0, bytes.Length );
    }

    public void WriteUInt32( UInt32 value )
    {
        var bytes = BitConverter.GetBytes( value );

        WriteBytes( bytes, 0, bytes.Length );
    }

    public void WriteUInt64( UInt64 value )
    {
        var bytes = BitConverter.GetBytes( value );

        WriteBytes( bytes, 0, bytes.Length );
    }
}
