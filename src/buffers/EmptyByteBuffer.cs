namespace Faactory.Channels.Buffers;

/// <summary>
/// A static class that provides an empty <see cref="IReadableByteBuffer"/> instance.
/// </summary>
public static class EmptyByteBuffer
{
    private static readonly Lazy<IReadableByteBuffer> lazyInstance = new(
        () => new ReadableByteBuffer( [] )
    );

    /// <summary>
    /// Gets an empty <see cref="IReadableByteBuffer"/> singleton instance (IsReadable == true).
    /// </summary>
    public static readonly IReadableByteBuffer Instance = lazyInstance.Value;
}
