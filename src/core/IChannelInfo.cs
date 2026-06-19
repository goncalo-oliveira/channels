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
    /// Gets the name of the channel
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the channel's ephemeral data
    /// </summary>
    IReadOnlyDictionary<string, object> Data { get; }

    /// <summary>
    /// Gets channel creation date
    /// </summary>
    DateTimeOffset Created { get; }

    /// <summary>
    /// Gets last received data timestamp
    /// </summary>
    DateTimeOffset? LastReceived { get; }

    /// <summary>
    /// Gets last sent data timestamp
    /// </summary>
    DateTimeOffset? LastSent { get; }
}
