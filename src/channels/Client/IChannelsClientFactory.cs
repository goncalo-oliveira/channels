namespace Faactory.Channels.Client;

/// <summary>
/// A factory for creating client instances
/// </summary>
public interface IChannelsClientFactory
{
    /// <summary>
    /// Creates a Channels client with the specified name
    /// </summary>
    /// <param name="name">The name of the client</param>
    /// <returns>A client instance that gives access to a channel for communication</returns>
    IChannelsClient Create( string name );
}
