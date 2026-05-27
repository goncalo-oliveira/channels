using System.Diagnostics.CodeAnalysis;

namespace Faactory.Channels.Buffers;

/// <summary>
/// Extension methods for <see cref="IReadableByteBuffer"/> to provide safe read operations that return a boolean indicating success or failure instead of throwing exceptions when there are not enough readable bytes available to perform the read operation.
/// </summary>
public static class ReadableByteBufferExtensions
{
    /// <summary>
    /// Creates a checkpoint for the current buffer position for speculative reads.
    /// The buffer returned is a view of the original buffer, sharing the same underlying data but with an independent offset.
    /// </summary>
    /// <returns>A readable checkpoint instance</returns>
    public static IReadableCheckpoint Checkpoint( this IReadableByteBuffer source )
        => new ReadableCheckpoint( source );

    /// <summary>
    /// Tries to read a boolean value from the buffer, returning false if there are not enough readable bytes to perform the read operation.
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <param name="value">The output boolean value</param>
    /// <returns>True if the read was successful, false otherwise</returns>
    public static bool TryReadBoolean( this IReadableByteBuffer source, out bool value )
    {
        if ( source.ReadableBytes < 1 )
        {
            value = default;

            return false;
        }

        value = source.ReadBoolean();

        return true;
    }

    /// <summary>
    /// Tries to read a byte value from the buffer, returning false if there are not enough readable bytes to perform the read operation.
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <param name="value">The output byte value</param>
    /// <returns>True if the read was successful, false otherwise</returns>
    public static bool TryReadByte( this IReadableByteBuffer source, out byte value )
    {
        if ( source.ReadableBytes < 1 )
        {
            value = default;

            return false;
        }

        value = source.ReadByte();

        return true;
    }

    /// <summary>
    /// Tries to read a range of bytes from the buffer, returning false if there are not enough readable bytes to perform the read operation.
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <param name="length">The number of bytes to read</param>
    /// <param name="value">The output byte array</param>
    /// <returns>True if the read was successful, false otherwise</returns>
    public static bool TryReadBytes( this IReadableByteBuffer source, int length, [MaybeNullWhen( false )] out byte[] value )
    {
        if ( source.ReadableBytes < length )
        {
            value = default;

            return false;
        }

        value = source.ReadBytes( length );

        return true;
    }

    /// <summary>
    /// Tries to read a range of bytes as <see cref="ReadOnlySpan{T}"/> from the buffer, returning false if there are not enough readable bytes to perform the read operation.
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <param name="length">The number of bytes to read</param>
    /// <param name="value">The output readonly span</param>
    /// <returns>True if the read was successful, false otherwise</returns>
    public static bool TryReadSpan( this IReadableByteBuffer source, int length, out ReadOnlySpan<byte> value )
    {
        if ( source.ReadableBytes < length )
        {
            value = default;

            return false;
        }

        value = source.ReadSpan( length );

        return true;
    }

    /// <summary>
    /// Tries to read a range of bytes as a <see cref="IReadableByteBuffer"/> from the buffer, returning false if there are not enough readable bytes to perform the read operation.
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <param name="length">The number of bytes to read</param>
    /// <param name="value">The output readable byte buffer</param>
    /// <returns>True if the read was successful, false otherwise</returns>
    public static bool TryReadByteBuffer( this IReadableByteBuffer source, int length, [MaybeNullWhen( false )] out IReadableByteBuffer value )
    {
        if ( source.ReadableBytes < length )
        {
            value = default;

            return false;
        }

        value = source.ReadByteBuffer( length );

        return true;
    }

    /// <summary>
    /// Tries to read a double value from the buffer, returning false if there are not enough readable bytes to perform the read operation.
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <param name="value">The output double value</param>
    /// <returns>True if the read was successful, false otherwise</returns>
    public static bool TryReadDouble( this IReadableByteBuffer source, out double value )
    {
        if ( source.ReadableBytes < 8 )
        {
            value = default;

            return false;
        }

        value = source.ReadDouble();

        return true;
    }

    /// <summary>
    /// Tries to read a float value from the buffer, returning false if there are not enough readable bytes to perform the read operation.
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <param name="value">The output float value</param>
    /// <returns>True if the read was successful, false otherwise</returns>
    public static bool TryReadSingle( this IReadableByteBuffer source, out float value )
    {
        if ( source.ReadableBytes < 4 )
        {
            value = default;

            return false;
        }

        value = source.ReadSingle();

        return true;
    }

    /// <summary>
    /// Tries to read a short value from the buffer, returning false if there are not enough readable bytes to perform the read operation.
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <param name="value">The output short value</param>
    /// <returns>True if the read was successful, false otherwise</returns>
    public static bool TryReadInt16( this IReadableByteBuffer source, out short value )
    {
        if ( source.ReadableBytes < 2 )
        {
            value = default;

            return false;
        }

        value = source.ReadInt16();

        return true;
    }

    /// <summary>
    /// Tries to read an int value from the buffer, returning false if there are not enough readable bytes to perform the read operation.
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <param name="value">The output int value</param>
    /// <returns>True if the read was successful, false otherwise</returns>
    public static bool TryReadInt32( this IReadableByteBuffer source, out int value )
    {
        if ( source.ReadableBytes < 4 )
        {
            value = default;

            return false;
        }

        value = source.ReadInt32();

        return true;
    }

    /// <summary>
    /// Tries to read a long value from the buffer, returning false if there are not enough readable bytes to perform the read operation.
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <param name="value">The output long value</param>
    /// <returns>True if the read was successful, false otherwise</returns>
    public static bool TryReadInt64( this IReadableByteBuffer source, out long value )
    {
        if ( source.ReadableBytes < 8 )
        {
            value = default;

            return false;
        }

        value = source.ReadInt64();

        return true;
    }

    /// <summary>
    /// Tries to read an ushort value from the buffer, returning false if there are not enough readable bytes to perform the read operation.
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <param name="value">The output ushort value</param>
    /// <returns>True if the read was successful, false otherwise</returns>
    public static bool TryReadUInt16( this IReadableByteBuffer source, out ushort value )
    {
        if ( source.ReadableBytes < 2 )
        {
            value = default;

            return false;
        }

        value = source.ReadUInt16();

        return true;
    }

    /// <summary>
    /// Tries to read an uint value from the buffer, returning false if there are not enough readable bytes to perform the read operation.
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <param name="value">The output uint value</param>
    /// <returns>True if the read was successful, false otherwise</returns>
    public static bool TryReadUInt32( this IReadableByteBuffer source, out uint value )
    {
        if ( source.ReadableBytes < 4 )
        {
            value = default;

            return false;
        }

        value = source.ReadUInt32();

        return true;
    }

    /// <summary>
    /// Tries to read an ulong value from the buffer, returning false if there are not enough readable bytes to perform the read operation.
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <param name="value">The output ulong value</param>
    /// <returns>True if the read was successful, false otherwise</returns>
    public static bool TryReadUInt64( this IReadableByteBuffer source, out ulong value )
    {
        if ( source.ReadableBytes < 8 )
        {
            value = default;

            return false;
        }

        value = source.ReadUInt64();

        return true;
    }
}
