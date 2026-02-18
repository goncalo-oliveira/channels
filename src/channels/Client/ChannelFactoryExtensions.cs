using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Faactory.Channels.Client;

internal sealed class ChannelName
{
    public required string Name { get; set; }
}

/// <summary>
/// Extension methods for creating Channels clients from an IChannelFactory
/// </summary>
public static class ChannelFactoryClientExtensions
{
    /// <summary>
    /// The default client name used when creating a Channels client without specifying a name
    /// </summary>
    public const string DefaultClientName = "__default_client";

    /// <summary>
    /// Creates a Channels client with the default client name
    /// </summary>
    /// <returns>A client instance that gives access to a channel for communication</returns>
    public static IChannelsClient CreateClientChannel( this IChannelFactory factory )
        => factory.CreateClientChannel( DefaultClientName );

    /// <summary>
    /// Creates a Channels client with the specified client name
    /// </summary>
    /// <param name="factory">The channel factory to create the client from</param>
    /// <param name="clientName">The name of the client</param>
    /// <returns>A client instance that gives access to a channel for communication</returns>
    public static IChannelsClient CreateClientChannel( this IChannelFactory factory, string clientName )
    {
        var scope = factory.ChannelServices.CreateScope();

        /*
        this gets the channel name (pipeline configuration) bound to the client name
        */
        var channelName = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<ChannelName>>()
            .Get( clientName ).Name;

        /*
        get the client options
        */
        var options = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<ChannelsClientOptions>>()
            .Get( clientName );

        if ( options.TransportMode == ChannelTransportMode.Tcp )
        {
            return new Tcp.TcpClient( scope, options, channelName );
        }

        if ( options.TransportMode == ChannelTransportMode.Udp )
        {
            return new Udp.ChannelsUdpClient( scope, options, channelName );
        }

        throw new NotSupportedException( $"Transport mode {options.TransportMode} is not supported." );
    }
}
