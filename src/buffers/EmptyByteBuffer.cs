namespace Faactory.Channels.Buffers;

/// <summary>
/// A static class that provides an empty <see cref="IByteBuffer"/> instance.
/// </summary>
internal static class EmptyByteBuffer
{
    private static readonly Lazy<IByteBuffer> lazyInstance = new(
        () => new WrappedByteBuffer( Array.Empty<byte>() )
    );

    /// <summary>
    /// Gets an empty <see cref="IByteBuffer"/> singleton instance (IsReadable == true).
    /// </summary>
    public static readonly IByteBuffer Instance = lazyInstance.Value;
}
