using System.Diagnostics.CodeAnalysis;

namespace Faactory.Channels;

/// <summary>
/// Interface for the channel registry.
/// </summary>
public interface IChannelRegistry
{
    /// <summary>
    /// Gets the collection of currently active channels.
    /// </summary>
    IReadOnlyCollection<IChannelHandle> Channels { get; }

    /// <summary>
    /// Tries to get the channel handle for the specified channel ID.
    /// </summary>
    /// <param name="channelId">The channel ID.</param>
    /// <param name="handle">The channel handle if found; otherwise, null.</param>
    /// <returns>True if the channel handle was found; otherwise, false.</returns>
    bool TryGet( string channelId, [MaybeNullWhen( false )] out IChannelHandle handle );
}
