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

    /// <summary>
    /// The current writing offset in the buffer, which also represents the length of the written portion.
    /// </summary>
    private int writeOffset = 0;

    private readonly Func<int, byte[]> Allocate;
    private readonly Action<byte[]>? Release;

    /// <summary>
    /// Gets the underlying byte array buffer. This is intended for internal use and should not be exposed publicly, as it may lead to unsafe modifications of the buffer. The buffer is shared with any views created from this instance, so modifying it directly can affect the integrity of those views.
    /// </summary>
    internal byte[] Buffer => buffer;

    /// <summary>
    /// Initializes a new instance of the <see cref="WritableByteBuffer"/> class with the default initial capacity and specified endianness.
    /// </summary>
    /// <param name="endianness">The endianness of the buffer</param>
    /// <param name="allocator">Optional custom allocator function for buffer allocation</param>
    /// <param name="releaser">Optional custom releaser action for buffer release</param>
    public WritableByteBuffer( Endianness endianness = Endianness.BigEndian, Func<int, byte[]>? allocator = null, Action<byte[]>? releaser = null )
        : this( InitialCapacity, endianness, allocator, releaser )
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="WritableByteBuffer"/> class with the specified initial capacity and endianness.
    /// </summary>
    /// <param name="capacity">The initial capacity of the buffer</param>
    /// <param name="endianness">The endianness of the buffer</param>
    /// <param name="allocator">Optional custom allocator function for buffer allocation</param>
    /// <param name="releaser">Optional custom releaser action for buffer release</param>
    public WritableByteBuffer( int capacity, Endianness endianness = Endianness.BigEndian, Func<int, byte[]>? allocator = null, Action<byte[]>? releaser = null )
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero( capacity, nameof( capacity ) );

        Allocate = allocator ?? ( size => new byte[size] );
        Release = releaser;

        buffer = Allocate( capacity );
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
    /// Creates a writable view of the buffer starting at the specified offset.
    /// The returned view shares the same underlying memory, allowing for zero-copy modifications.
    /// The offset must be within the bounds of the buffer's capacity.
    /// Modifying the returned view will affect the original buffer, and vice versa.
    /// The returned view is limited to the portion of the buffer starting from the specified offset to the end of the used portion of the buffer.
    /// </summary>
    /// <param name="offset">The offset from which to create the writable view</param>
    /// <returns>A writable buffer view starting at the specified offset</returns>
    public IWritableByteBuffer At( int offset )
    {
        ObjectDisposedException.ThrowIf( buffer is null, this );
        ArgumentOutOfRangeException.ThrowIfNegative( offset, nameof( offset ) );
        ArgumentOutOfRangeException.ThrowIfGreaterThan( offset, writeOffset, nameof( offset ) );

        return new WritableByteBufferView( this, offset );
    }

    /// <summary>
    /// Discards all written bytes and reallocates the buffer to its initial capacity.
    /// </summary>
    /// <returns>The same IWritableByteBuffer instance to allow fluent syntax</returns>
    public IWritableByteBuffer Clear()
    {
        ObjectDisposedException.ThrowIf( buffer is null, this );

        if ( buffer.Length > InitialCapacity )
        {
            Release?.Invoke( buffer );
            buffer = Allocate( InitialCapacity );
        }

        writeOffset = 0;

        return this;
    }

    /// <summary>
    /// Releases any resources associated with the buffer. If a custom releaser was provided, it will be invoked with the current buffer.
    /// </summary>
    public void Dispose()
    {
        if ( buffer is not null )
        {
            Release?.Invoke( buffer );
            buffer = null!;
        }
    }

    /// <summary>
    /// Reserves a contiguous block of bytes for writing and moves the writing offset forward by the specified length.
    /// </summary>
    /// <param name="length">The number of bytes to reserve for writing</param>
    /// <returns>The same IWritableByteBuffer instance to allow fluent syntax</returns>
    public IWritableByteBuffer Reserve( int length )
    {
        ObjectDisposedException.ThrowIf( buffer is null, this );
        ArgumentOutOfRangeException.ThrowIfNegative( length, nameof( length ) );

        EnsureCapacity( length );

        writeOffset += length;

        return this;
    }

    /// <summary>
    /// Truncates the buffer to the specified position, effectively discarding all written bytes beyond that point. Underlying buffer capacity remains unchanged.
    /// </summary>
    /// <param name="offset">The offset to truncate to; defaults to the beginning of the buffer</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the offset is negative or greater than the used portion of the buffer</exception>
    /// <returns>The same IWritableByteBuffer instance to allow fluent syntax</returns>
    public IWritableByteBuffer Truncate( int offset = 0 )
    {
        ArgumentOutOfRangeException.ThrowIfNegative( offset, nameof( offset ) );
        ArgumentOutOfRangeException.ThrowIfGreaterThan( offset, writeOffset, nameof( offset ) );

        writeOffset = offset;

        return this;
    }

    /// <summary>
    /// Creates a readable view of the currently written portion of the buffer.
    /// </summary>
    /// <remarks>
    /// Returns a zero-copy readable view over the currently written portion of the buffer.
    /// The returned view shares the same underlying memory.
    /// It must not be used after the writable buffer is modified (e.g., written to or compacted),
    /// as the view may no longer represent the same logical data.
    /// </remarks>
    /// <returns>A readable buffer view of the currently written portion</returns>
    public IReadableByteBuffer AsReadableView()
    {
        ObjectDisposedException.ThrowIf( buffer is null, this );

        return new ReadableByteBuffer( buffer, 0, writeOffset, Endianness );
    }

    /// <summary>
    /// Compacts the buffer by discarding bytes up to the specified offset.
    /// </summary>
    /// <param name="offset">The offset up to which bytes should be discarded</param>
    /// <returns>The same IWritableByteBuffer instance to allow fluent syntax</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the offset is negative or greater than the used portion of the buffer</exception>
    public IWritableByteBuffer Compact( int offset )
    {
        ObjectDisposedException.ThrowIf(buffer is null, this);

        ArgumentOutOfRangeException.ThrowIfNegative( offset, nameof( offset ) );
        ArgumentOutOfRangeException.ThrowIfGreaterThan( offset, writeOffset, nameof( offset ) );

        if ( offset == 0 )
        {
            return this;
        }

        var remaining = writeOffset - offset;

        if ( remaining > 0 )
        {
            Array.Copy( buffer, offset, buffer, 0, remaining );
        }

        writeOffset = remaining;

        return this;
    }

    /// <summary>
    /// Gets the used portion of the buffer as a byte[]
    /// </summary>
    /// <returns>A byte[] value</returns>
    public byte[] ToArray()
    {
        ObjectDisposedException.ThrowIf( buffer is null, this );

        var dest = new byte[Length];

        Array.Copy( buffer, 0, dest, 0, Length );

        return dest;
    }

    /// <summary>
    /// Gets the used portion of the buffer as a <see cref="ReadOnlySpan{T}"/>
    /// </summary>
    /// <returns>A <see cref="ReadOnlySpan{T}"/> representing the used portion of the buffer</returns>
    public ReadOnlySpan<byte> AsSpan()
    {
        ObjectDisposedException.ThrowIf( buffer is null, this );

        return buffer.AsSpan( 0, writeOffset );
    }

    private void EnsureCapacity( int additionalLength )
    {
        ObjectDisposedException.ThrowIf( buffer is null, this );
        ArgumentOutOfRangeException.ThrowIfNegative( additionalLength, nameof( additionalLength ) );

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

        var newBuffer = Allocate( newSize );

        Array.Copy( buffer, 0, newBuffer, 0, writeOffset );

        Release?.Invoke( buffer );

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

    internal void WriteDouble( Span<byte> span, double value )
    {
        if ( Endianness == Endianness.BigEndian )
        {
            BinaryPrimitives.WriteDoubleBigEndian( span, value );
        }
        else
        {
            BinaryPrimitives.WriteDoubleLittleEndian( span, value );
        }
    }

    internal void WriteSingle( Span<byte> span, float value )
    {
        if ( Endianness == Endianness.BigEndian )
        {
            BinaryPrimitives.WriteSingleBigEndian( span, value );
        }
        else
        {
            BinaryPrimitives.WriteSingleLittleEndian( span, value );
        }
    }

    internal void WriteInt64( Span<byte> span, long value )
    {
        if ( Endianness == Endianness.BigEndian )
        {
            BinaryPrimitives.WriteInt64BigEndian( span, value );
        }
        else
        {
            BinaryPrimitives.WriteInt64LittleEndian( span, value );
        }
    }

    internal void WriteInt16( Span<byte> span, short value )
    {
        if ( Endianness == Endianness.BigEndian )
        {
            BinaryPrimitives.WriteInt16BigEndian( span, value );
        }
        else
        {
            BinaryPrimitives.WriteInt16LittleEndian( span, value );
        }
    }

    internal void WriteInt32( Span<byte> span, int value )
    {
        if ( Endianness == Endianness.BigEndian )
        {
            BinaryPrimitives.WriteInt32BigEndian( span, value );
        }
        else
        {
            BinaryPrimitives.WriteInt32LittleEndian( span, value );
        }
    }

    internal void WriteUInt16( Span<byte> span, ushort value )
    {
        if ( Endianness == Endianness.BigEndian )
        {
            BinaryPrimitives.WriteUInt16BigEndian( span, value );
        }
        else
        {
            BinaryPrimitives.WriteUInt16LittleEndian( span, value );
        }
    }

    internal void WriteUInt32( Span<byte> span, uint value )
    {
        if ( Endianness == Endianness.BigEndian )
        {
            BinaryPrimitives.WriteUInt32BigEndian( span, value );
        }
        else
        {
            BinaryPrimitives.WriteUInt32LittleEndian( span, value );
        }
    }

    internal void WriteUInt64( Span<byte> span, ulong value )
    {
        if ( Endianness == Endianness.BigEndian )
        {
            BinaryPrimitives.WriteUInt64BigEndian( span, value );
        }
        else
        {
            BinaryPrimitives.WriteUInt64LittleEndian( span, value );
        }
    }

    /// <summary>
    /// Writes a double value to the buffer, taking into account the endianness of the buffer.
    /// </summary>
    /// <param name="value">The double value to write</param>
    /// <returns>The current buffer instance</returns>
    public IWritableByteBuffer WriteDouble( double value )
    {
        WritePrimitive( value, sizeof( double ), WriteDouble );

        return this;
    }

    /// <summary>
    /// Writes a float value to the buffer, taking into account the endianness of the buffer.
    /// </summary>
    /// <param name="value">The float value to write</param>
    /// <returns>The current buffer instance</returns>
    public IWritableByteBuffer WriteSingle( float value )
    {
        WritePrimitive( value, sizeof( float ), WriteSingle );

        return this;
    }

    /// <summary>
    /// Writes a short (Int16) value to the buffer, taking into account the endianness of the buffer.
    /// </summary>
    /// <param name="value">The short value to write</param>
    /// <returns>The current buffer instance</returns>
    public IWritableByteBuffer WriteInt16( short value )
    {
        WritePrimitive( value, sizeof( short ), WriteInt16 );

        return this;
    }

    /// <summary>
    /// Writes an int (Int32) value to the buffer, taking into account the endianness of the buffer.
    /// </summary>
    /// <param name="value">The int value to write</param>
    /// <returns>The current buffer instance</returns>
    public IWritableByteBuffer WriteInt32( int value )
    {
        WritePrimitive( value, sizeof( int ), WriteInt32 );

        return this;
    }

    /// <summary>
    /// Writes a long (Int64) value to the buffer, taking into account the endianness of the buffer.
    /// </summary>
    /// <param name="value">The long value to write</param>
    /// <returns>The current buffer instance</returns>
    public IWritableByteBuffer WriteInt64( long value )
    {
        WritePrimitive( value, sizeof( long ), WriteInt64 );

        return this;
    }

    /// <summary>
    /// Writes an unsigned short (UInt16) value to the buffer, taking into account the endianness of the buffer.
    /// </summary>
    /// <param name="value">The unsigned short value to write</param>
    /// <returns>The current buffer instance</returns>
    public IWritableByteBuffer WriteUInt16( ushort value )
    {
        WritePrimitive( value, sizeof( ushort ), WriteUInt16 );

        return this;
    }

    /// <summary>
    /// Writes an unsigned int (UInt32) value to the buffer, taking into account the endianness of the buffer.
    /// </summary>
    /// <param name="value">The unsigned int value to write</param>
    /// <returns>The current buffer instance</returns>
    public IWritableByteBuffer WriteUInt32( uint value )
    {
        WritePrimitive( value, sizeof( uint ), WriteUInt32 );

        return this;
    }

    /// <summary>
    /// Writes an unsigned long (UInt64) value to the buffer, taking into account the endianness of the buffer.
    /// </summary>
    /// <param name="value">The unsigned long value to write</param>
    /// <returns>The current buffer instance</returns>
    public IWritableByteBuffer WriteUInt64( ulong value )
    {
        WritePrimitive( value, sizeof( ulong ), WriteUInt64 );

        return this;
    }
}
