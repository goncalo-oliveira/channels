using System.Buffers;

namespace Faactory.Channels.Buffers;

internal sealed class WritableByteBufferView( WritableByteBuffer buffer, int offset, int bufferLimit ) : IWritableByteBufferView
{
    /// <summary>
    /// The current writing offset in the view.
    /// </summary>
    private int writeOffset = 0;

    public int Capacity => Length; // The capacity of the view is its length, as it cannot grow beyond its bounds.

    public Endianness Endianness => buffer.Endianness;

    /// <summary>
    /// Gets the length of the view.
    /// This is the distance between the view start offset and its limit.
    /// </summary>
    public int Length => bufferLimit - offset;

    public int WritableBytes => Length - writeOffset;

    public Memory<byte> AsMemory()
    {
        return buffer.AsMemory().Slice( offset, Length );
    }

    public Span<byte> AsSpan()
    {
        return buffer.AsSpan()[offset..bufferLimit];
    }


    // explicit implementation
    ReadOnlyMemory<byte> IByteBuffer.AsMemory() => AsMemory();
    ReadOnlySpan<byte> IByteBuffer.AsSpan() => AsSpan();

    public IReadableByteBuffer AsReadableView()
    {
        return new ReadableByteBuffer( buffer.Buffer, offset, Length, buffer.Endianness );
    }

    public IWritableByteBuffer Clear()
        => throw new NotSupportedException( "Cannot clear a writable view." );

    public IWritableByteBuffer Compact()
        => throw new NotSupportedException( "Cannot compact a writable view." );

    public IWritableByteBufferView CreateView( int viewOffset, int length )
    {
        ThrowIfParentDisposed();

        ArgumentOutOfRangeException.ThrowIfNegative( viewOffset, nameof( viewOffset ) );
        ArgumentOutOfRangeException.ThrowIfNegative( length, nameof( length ) );
        ArgumentOutOfRangeException.ThrowIfGreaterThan( length, Length - viewOffset, nameof( length ) );

        return new WritableByteBufferView(
            buffer,
            offset + viewOffset,
            offset + viewOffset + length
        );
    }

    public void Dispose()
    {
        // No resources to dispose in the view
    }

    public IWritableByteBuffer Rebase( int offset )
        => throw new NotSupportedException( "Cannot rebase a writable view." );

    public IWritableByteBuffer Reserve( int length )
    {
        ThrowIfParentDisposed();

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
        ThrowIfParentDisposed();

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
        ArgumentOutOfRangeException.ThrowIfNegative( size, nameof( size ) );

        if ( offset + writeOffset + size > bufferLimit )
        {
            throw new InvalidOperationException( "Writable view cannot write beyond its bounds." );
        }
    }

    private void ThrowIfParentDisposed()
        => _ = buffer.Capacity;
}
