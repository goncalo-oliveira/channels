using System.Buffers;

namespace Faactory.Channels.Buffers;

internal sealed class WritableByteBufferView( WritableByteBuffer buffer, int offset ) : IWritableByteBuffer
{
    /// <summary>
    /// The limit of the view, which is the end of the used portion of the parent buffer. The view cannot write beyond this limit.
    /// This is calculated at the time of view creation and does not change even if the parent buffer is modified after the view is created.
    /// </summary>
    private readonly int limit = buffer.Length;

    /// <summary>
    /// The current writing offset in the view.
    /// </summary>
    private int writeOffset = 0;

    public Endianness Endianness => buffer.Endianness;

    /// <summary>
    /// The length of the writable portion of the view, which is the distance from the current offset to the limit.
    /// The view can write up to this length before it would extend beyond the limit.
    /// </summary>
    public int Length => limit - offset;

    public ReadOnlySpan<byte> AsSpan()
    {
        return buffer.AsSpan()[offset..limit];
    }

    public IReadableByteBuffer AsReadableView()
        => throw new NotSupportedException( "Cannot create a readable view from a writable view." );

    public IWritableByteBuffer At( int offset )
        => throw new NotSupportedException( "Cannot create a writable view from a writable view." );

    public IWritableByteBuffer Clear()
        => throw new NotSupportedException( "Cannot clear a writable view." );

    public IWritableByteBuffer Compact( int offset)
        => throw new NotSupportedException( "Cannot compact a writable view." );

    public void Dispose()
    {
        // No resources to dispose in the view
    }

    public IWritableByteBuffer Reserve( int length )
    {
        EnsureSpace( length );

        writeOffset += length;

        return this;
    }

    public byte[] ToArray()
    {
        return AsSpan().ToArray();
    }

    public IWritableByteBuffer Truncate( int offset = 0 )
    {
        ArgumentOutOfRangeException.ThrowIfNegative( offset, nameof( offset ) );
        ArgumentOutOfRangeException.ThrowIfGreaterThan( offset, writeOffset, nameof( offset ) );

        writeOffset = offset;

        return this;
    }

    public IWritableByteBuffer WriteBoolean( bool value )
        => WriteByte( value ? (byte)1 : (byte)0 );

    public IWritableByteBuffer WriteByte( byte value )
    {
        EnsureSpace( 1 );

        buffer.Buffer[offset + writeOffset] = value;
        writeOffset += 1;

        return this;
    }

    public IWritableByteBuffer WriteBytes( byte[] value, int startIndex, int length )
    {
        EnsureSpace( length );

        Array.Copy( value, startIndex, buffer.Buffer, offset + writeOffset, length );

        writeOffset += length;

        return this;
    }

    public IWritableByteBuffer WriteBytes( IByteBuffer value )
        => WriteBytes( value.AsSpan() );

    public IWritableByteBuffer WriteBytes( ReadOnlySpan<byte> value )
    {
        EnsureSpace( value.Length );

        value.CopyTo( buffer.Buffer.AsSpan( offset + writeOffset, value.Length ) );

        writeOffset += value.Length;

        return this;
    }

    public IWritableByteBuffer WriteDouble( double value )
    {
        WritePrimitive( value, sizeof( double ), buffer.WriteDouble );

        return this;
    }

    public IWritableByteBuffer WriteInt16( short value )
    {
        WritePrimitive( value, sizeof( short ), buffer.WriteInt16 );

        return this;
    }

    public IWritableByteBuffer WriteInt32( int value )
    {
        WritePrimitive( value, sizeof( int ), buffer.WriteInt32 );

        return this;
    }

    public IWritableByteBuffer WriteInt64( long value )
    {
        WritePrimitive( value, sizeof( long ), buffer.WriteInt64 );

        return this;
    }

    public IWritableByteBuffer WriteSingle( float value )
    {
        WritePrimitive( value, sizeof( float ), buffer.WriteSingle );

        return this;
    }

    public IWritableByteBuffer WriteUInt16( ushort value )
    {
        WritePrimitive( value, sizeof( ushort ), buffer.WriteUInt16 );

        return this;
    }

    public IWritableByteBuffer WriteUInt32( uint value )
    {
        WritePrimitive( value, sizeof( uint ), buffer.WriteUInt32 );

        return this;
    }

    public IWritableByteBuffer WriteUInt64( ulong value )
    {
        WritePrimitive( value, sizeof( ulong ), buffer.WriteUInt64 );

        return this;
    }

    private void WritePrimitive<T>( T value, int size, SpanAction<byte, T> writer )
    {
        EnsureSpace( size );

        var span = buffer.Buffer.AsSpan( offset + writeOffset, size );

        writer( span, value );

        writeOffset += size;
    }


    private void EnsureSpace( int size )
    {
        if ( offset + writeOffset + size > limit )
        {
            throw new InvalidOperationException( "Writable view cannot extend buffer." );
        }
    }
}
