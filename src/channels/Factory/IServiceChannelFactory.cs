namespace Faactory.Channels;

/// <summary>
/// A factory for creating service channel instances
/// </summary>
internal interface IServiceChannelFactory
{
    /// <summary>
    /// Creates a new service channel instance
    /// </summary>
    Task<IChannel> CreateChannelAsync( System.Net.Sockets.Socket socket );
}
