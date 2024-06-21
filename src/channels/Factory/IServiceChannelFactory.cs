namespace Faactory.Channels;

/// <summary>
/// A factory for creating service channel instances
/// </summary>
internal interface IServiceChannelFactory
{
    /// <summary>
    /// Creates a new service channel instance
    /// </summary>
    IChannel CreateChannel( System.Net.Sockets.Socket socket, string? name = null );
}
