namespace Faactory.Channels.Client;

public interface IChannelsClientFactory
{
    IChannelsClient Create( string name );
}
