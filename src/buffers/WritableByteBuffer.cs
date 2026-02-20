using System.Buffers;
using System.Buffers.Binary;

namespace Faactory.Channels.Buffers;

/// <summary>
/// A writable byte buffer implementation that allows writing various primitive types and byte arrays with automatic resizing
/// </summary>
public sealed class WritableByteBuffer : IWritableByteBuffer
{
    internal const int InitialCapacity = 1024;

    private byte[] buffer;
    private int writeOffset = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="WritableByteBuffer"/> class with the default initial capacity and specified endianness.
    /// </summary>
    /// <param name="endianness">The endianness of the buffer</param>
    public WritableByteBuffer( Endianness endianness = Endianness.BigEndian )
        : this( InitialCapacity, endianness )
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="WritableByteBuffer"/> class with the specified initial capacity and endianness.
    /// </summary>
    /// <param name="capacity">The initial capacity of the buffer</param>
    /// <param name="endianness">The endianness of the buffer</param>
    public WritableByteBuffer( int capacity, Endianness endianness = Endianness.BigEndian )
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero( capacity, nameof( capacity ) );

        buffer = new byte[capacity];
        Endianness = endianness;
    }

    /// <summary>
    /// Gets the endianness of the buffer, which determines how multi-byte values are written.
    /// </summary>
    public Endianness Endianness { get; }

    /// <summary>
    /// Gets the length of the used portion of the buffer.
    /// </summary>
    public int Length => writeOffset;

    /// <summary>
    /// Discards all written bytes and reallocates the buffer to its initial capacity.
    /// </summary>
    /// <returns>The same IWritableByteBuffer instance to allow fluent syntax</returns>
    public IWritableByteBuffer Clear()
    {
        buffer = new byte[InitialCapacity];
        writeOffset = 0;

        return this;
    }

    /// <summary>
    /// Resets the writing offset to the beginning of the buffer, effectively discarding all written bytes. Current buffer capacity remains unchanged.
    /// </summary>
    /// <returns>The same IWritableByteBuffer instance to allow fluent syntax</returns>
    public IWritableByteBuffer ResetOffset()
    {
        writeOffset = 0;

        return this;
    }

    /// <summary>
    /// Gets the entire buffer as a byte[] no matter where the reading/writing offset is
    /// </summary>
    /// <returns>A byte[] value</returns>
    public byte[] ToArray()
    {
        var dest = new byte[Length];

        Array.Copy( buffer, 0, dest, 0, Length );

        return dest;
    }

    /// <summary>
    /// Gets the used portion of the buffer as a <see cref="ReadOnlySpan{T}"/>
    /// </summary>
    /// <returns>A <see cref="ReadOnlySpan{T}"/> representing the used portion of the buffer</returns>
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

    /// <summary>
    /// Writes a boolean value to the buffer as a single byte (1 for true, 0 for false).
    /// </summary>
    /// <param name="value">The boolean value to write</param>
    /// <returns>The current buffer instance</returns>
    public IWritableByteBuffer WriteBoolean( bool value )
        => WriteByte( value ? (byte)1 : (byte)0 );

    /// <summary>
    /// Writes a byte value to the buffer.
    /// </summary>
    /// <param name="value">The byte value to write</param>
    /// <returns>The current buffer instance</returns>
    public IWritableByteBuffer WriteByte( byte value )
    {
        EnsureCapacity( 1 );

        buffer[writeOffset++] = value;

        return this;
    }

    /// <summary>
    /// Writes a byte array to the buffer.
    /// </summary>
    /// <param name="value">The byte array to write</param>
    /// <param name="startIndex">The starting index in the byte array</param>
    /// <param name="length">The number of bytes to write</param>
    /// <returns>The current buffer instance</returns>
    public IWritableByteBuffer WriteBytes( byte[] value, int startIndex, int length )
    {
        EnsureCapacity( length );

        Array.Copy( value, startIndex, buffer, writeOffset, length );

        writeOffset += length;

        return this;
    }

    /// <summary>
    /// Writes the contents of another <see cref="IByteBuffer"/> to this buffer.
    /// </summary>
    /// <param name="value">The buffer whose contents are to be written</param>
    /// <returns>The current buffer instance</returns>
    public IWritableByteBuffer WriteBytes( IByteBuffer value )
        => WriteBytes( value.AsSpan() );

    /// <summary>
    /// Writes the contents of a <see cref="ReadOnlySpan{T}"/> to the buffer.
    /// </summary>
    /// <param name="value">The span containing the bytes to write</param>
    /// <returns>The current buffer instance</returns>
    public IWritableByteBuffer WriteBytes( ReadOnlySpan<byte> value )
    {
        EnsureCapacity( value.Length );

        value.CopyTo( buffer.AsSpan( writeOffset ) );

        writeOffset += value.Length;

        return this;
    }

    /// <summary>
    /// Writes a double value to the buffer, taking into account the endianness of the buffer.
    /// </summary>
    /// <param name="value">The double value to write</param>
    /// <returns>The current buffer instance</returns>
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

    /// <summary>
    /// Writes a float value to the buffer, taking into account the endianness of the buffer.
    /// </summary>
    /// <param name="value">The float value to write</param>
    /// <returns>The current buffer instance</returns>
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

    /// <summary>
    /// Writes a short (Int16) value to the buffer, taking into account the endianness of the buffer.
    /// </summary>
    /// <param name="value">The short value to write</param>
    /// <returns>The current buffer instance</returns>
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

    /// <summary>
    /// Writes an int (Int32) value to the buffer, taking into account the endianness of the buffer.
    /// </summary>
    /// <param name="value">The int value to write</param>
    /// <returns>The current buffer instance</returns>
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

    /// <summary>
    /// Writes a long (Int64) value to the buffer, taking into account the endianness of the buffer.
    /// </summary>
    /// <param name="value">The long value to write</param>
    /// <returns>The current buffer instance</returns>
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

    /// <summary>
    /// Writes an unsigned short (UInt16) value to the buffer, taking into account the endianness of the buffer.
    /// </summary>
    /// <param name="value">The unsigned short value to write</param>
    /// <returns>The current buffer instance</returns>
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

    /// <summary>
    /// Writes an unsigned int (UInt32) value to the buffer, taking into account the endianness of the buffer.
    /// </summary>
    /// <param name="value">The unsigned int value to write</param>
    /// <returns>The current buffer instance</returns>
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

    /// <summary>
    /// Writes an unsigned long (UInt64) value to the buffer, taking into account the endianness of the buffer.
    /// </summary>
    /// <param name="value">The unsigned long value to write</param>
    /// <returns>The current buffer instance</returns>
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
