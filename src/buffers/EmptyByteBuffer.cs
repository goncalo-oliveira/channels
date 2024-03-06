namespace Faactory.Channels.Buffers;

/// <summary>
/// A static class that provides an empty <see cref="IByteBuffer"/> instance.
/// </summary>
public static class EmptyByteBuffer
{
    private static readonly Lazy<IByteBuffer> lazyInstance = new(
        () => new WrappedByteBuffer( [] )
    );

    /// <summary>
    /// Gets an empty <see cref="IByteBuffer"/> singleton instance (IsReadable == true).
    /// </summary>
    public static readonly IByteBuffer Instance = lazyInstance.Value;
}
