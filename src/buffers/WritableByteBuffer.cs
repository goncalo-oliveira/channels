using System.Buffers;
using System.Buffers.Binary;

namespace Faactory.Channels.Buffers;

public sealed class WritableByteBuffer( int capacity, Endianness endianness = Endianness.BigEndian ) : IWritableByteBuffer
{
    internal const int InitialCapacity = 1024;

    private byte[] buffer = new byte[capacity];
    private int writeOffset = 0;

    public WritableByteBuffer( Endianness endianness = Endianness.BigEndian )
        : this( InitialCapacity, endianness )
    { }

    public Endianness Endianness { get; } = endianness;

    public int Length => writeOffset;

    /// <summary>
    /// Resets the writing offset to the beginning of the buffer, effectively discarding all written bytes. Current buffer capacity remains unchanged.
    /// </summary>
    /// <returns>The current buffer instance</returns>
    public IWritableByteBuffer ResetOffset()
    {
        writeOffset = 0;

        return this;
    }

    public byte[] ToArray()
    {
        var dest = new byte[Length];

        Array.Copy( buffer, 0, dest, 0, Length );

        return dest;
    }

    public ReadOnlySpan<byte> AsSpan()
        => buffer.AsSpan( 0, writeOffset );

    private void EnsureCapacity( int additionalLength )
    {
        int required = writeOffset + additionalLength;

        if ( required <= buffer.Length )
        {
            return;
        }

        int newSize = buffer.Length;

        while ( newSize < required )
        {
            newSize *= 2;
        }

        var newBuffer = new byte[newSize];

        Array.Copy( buffer, 0, newBuffer, 0, writeOffset );

        buffer = newBuffer;
    }

    private void WritePrimitive<T>( T value, int size, SpanAction<byte, T> writer )
    {
        EnsureCapacity( size );

        var span = buffer.AsSpan( writeOffset, size );

        writer( span, value );

        writeOffset += size;
    }

    public IWritableByteBuffer WriteBoolean( bool value )
        => WriteByte( value ? (byte)1 : (byte)0 );

    public IWritableByteBuffer WriteByte( byte value )
    {
        EnsureCapacity( 1 );

        buffer[writeOffset++] = value;

        return this;
    }

    public IWritableByteBuffer WriteBytes( byte[] value, int startIndex, int length )
    {
        EnsureCapacity( length );

        Array.Copy( value, startIndex, buffer, writeOffset, length );

        writeOffset += length;

        return this;
    }

    public IWritableByteBuffer WriteByteBuffer( IReadableByteBuffer value )
    {
        var bytes = value.ToArray();

        return WriteBytes( bytes, 0, bytes.Length );
    }

    public IWritableByteBuffer WriteBytes( ReadOnlySpan<byte> value )
    {
        EnsureCapacity( value.Length );

        value.CopyTo( buffer.AsSpan( writeOffset ) );

        writeOffset += value.Length;

        return this;
    }

    public IWritableByteBuffer WriteDouble( double value )
    {
        WritePrimitive( value, sizeof( double ), ( span, val ) =>
        {
            if ( Endianness == Endianness.BigEndian )
            {
                BinaryPrimitives.WriteDoubleBigEndian( span, val );
            }
            else
            {
                BinaryPrimitives.WriteDoubleLittleEndian( span, val );
            }
        } );

        return this;
    }

    public IWritableByteBuffer WriteSingle( float value )
    {
        WritePrimitive( value, sizeof( float ), ( span, val ) =>
        {
            if ( Endianness == Endianness.BigEndian )
            {
                BinaryPrimitives.WriteSingleBigEndian( span, val );
            }
            else
            {
                BinaryPrimitives.WriteSingleLittleEndian( span, val );
            }
        } );

        return this;
    }

    public IWritableByteBuffer WriteInt16( short value )
    {
        WritePrimitive( value, sizeof( short ), ( span, val ) =>
        {
            if ( Endianness == Endianness.BigEndian )
            {
                BinaryPrimitives.WriteInt16BigEndian( span, val );
            }
            else
            {
                BinaryPrimitives.WriteInt16LittleEndian( span, val );
            }
        } );

        return this;
    }

    public IWritableByteBuffer WriteInt32( int value )
    {
        WritePrimitive( value, sizeof( int ), ( span, val ) =>
        {
            if ( Endianness == Endianness.BigEndian )
            {
                BinaryPrimitives.WriteInt32BigEndian( span, val );
            }
            else
            {
                BinaryPrimitives.WriteInt32LittleEndian( span, val );
            }
        } );

        return this;
    }

    public IWritableByteBuffer WriteInt64( long value )
    {
        WritePrimitive( value, sizeof( long ), ( span, val ) =>
        {
            if ( Endianness == Endianness.BigEndian )
            {
                BinaryPrimitives.WriteInt64BigEndian( span, val );
            }
            else
            {
                BinaryPrimitives.WriteInt64LittleEndian( span, val );
            }
        } );

        return this;
    }

    public IWritableByteBuffer WriteUInt16( ushort value )
    {
        WritePrimitive( value, sizeof( ushort ), ( span, val ) =>
        {
            if ( Endianness == Endianness.BigEndian )
            {
                BinaryPrimitives.WriteUInt16BigEndian( span, val );
            }
            else
            {
                BinaryPrimitives.WriteUInt16LittleEndian( span, val );
            }
        } );

        return this;
    }

    public IWritableByteBuffer WriteUInt32( uint value )
    {
        WritePrimitive( value, sizeof( uint ), ( span, val ) =>
        {
            if ( Endianness == Endianness.BigEndian )
            {
                BinaryPrimitives.WriteUInt32BigEndian( span, val );
            }
            else
            {
                BinaryPrimitives.WriteUInt32LittleEndian( span, val );
            }
        } );

        return this;
    }

    public IWritableByteBuffer WriteUInt64( ulong value )
    {
        WritePrimitive( value, sizeof( ulong ), ( span, val ) =>
        {
            if ( Endianness == Endianness.BigEndian )
            {
                BinaryPrimitives.WriteUInt64BigEndian( span, val );
            }
            else
            {
                BinaryPrimitives.WriteUInt64LittleEndian( span, val );
            }
        } );

        return this;
    }
}
