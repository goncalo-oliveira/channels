namespace Faactory.Channels;

/// <summary>
/// Channel read-only information
/// </summary>
public interface IChannelInfo
{
    /// <summary>
    /// Gets the channel identifier
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the channel's metadata
    /// </summary>
    IReadOnlyDictionary<string, string> Data { get; }
}
