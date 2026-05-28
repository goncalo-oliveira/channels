namespace Faactory.Channels.Buffers;

/// <summary>
/// Provides extension methods for IWritableByteBuffer instances
/// </summary>
public static class WritableByteBufferExtensions
{
    /// <summary>
    /// Writes a range of bytes
    /// </summary>
    /// <param name="source">The source buffer</param>
    /// <param name="value">The byte[] value</param>
    public static IWritableByteBuffer WriteBytes( this IWritableByteBuffer source, byte[] value )
        => source.WriteBytes( value, 0, value.Length );
}
