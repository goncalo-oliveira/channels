namespace Faactory.Channels.Client;

public static class ChannelsClientFactoryExtensions
{
    /// <summary>
    /// Creates a Channels client with the default client name
    /// </summary>
    /// <returns>A client instance that gives access to a channel for communication</returns>
    public static IChannelsClient Create( this IChannelsClientFactory factory )
        => factory.Create( ChannelsClientFactory.DefaultClientName );
}
