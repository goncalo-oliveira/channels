namespace Faactory.Channels;

/// <summary>
/// A null implementation of the IChannelRegistrar interface that does nothing when registering a channel.
/// </summary>
public sealed class NullChannelRegistrar : IChannelRegistrar
{
    /// <summary>
    /// Gets the singleton instance of the NullChannelRegistrar.
    /// </summary>
    public static NullChannelRegistrar Instance { get; } = new();

    /// <summary>
    /// Registers a channel. This implementation does nothing.
    /// </summary>
    /// <param name="channel">The channel to register.</param>
    public void Register( IChannel channel )
    {
        // do nothing.
    }
}
