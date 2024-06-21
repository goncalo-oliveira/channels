namespace Faactory.Channels.Client;

public interface IChannelsClient : IDisposable
{
    IChannel Channel { get; }
    string Name { get; }

    Task CloseAsync();
}
