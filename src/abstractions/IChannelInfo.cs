namespace Faactory.Channels;

/// <summary>
/// Channel info
/// </summary>
public interface IChannelInfo
{
    string Id { get; }
    IReadOnlyDictionary<string, string> Data { get; }
}
