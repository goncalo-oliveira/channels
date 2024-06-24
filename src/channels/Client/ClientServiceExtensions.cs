using Faactory.Channels;
using Faactory.Channels.Client;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class ChannelsClientServiceExtensions
{
    /// <summary>
    /// Adds a channels client to the service collection and binds it to the default channel (pipeline configuration)
    /// </summary>
    /// <param name="clientName">The name of the client</param>
    /// <param name="configure">A delegate to configure the client options</param>
    public static IServiceCollection AddChannelsNamedClient( this IServiceCollection services, string clientName, Action<ChannelsClientOptions> configure )
        => services.AddChannelsNamedClient( clientName, ChannelBuilder.DefaultChannelName, configure );

    /// <summary>
    /// Adds a channels client to the service collection and binds it to the default channel (pipeline configuration)
    /// </summary>
    /// <param name="clientName">The name of the client</param>
    /// <param name="hostUrl">The URL of the service host</param>
    /// <exception cref="ArgumentException">Thrown when the host URL is not a valid URI</exception>
    /// <exception cref="NotSupportedException">Thrown when the transport mode is not supported</exception>
    public static IServiceCollection AddChannelsNamedClient( this IServiceCollection services, string clientName, string hostUrl )
        => services.AddChannelsNamedClient( clientName, ChannelBuilder.DefaultChannelName, hostUrl );

    /// <summary>
    /// Adds a channels client to the service collection
    /// </summary>
    /// <param name="clientName">The name of the client</param>
    /// <param name="channelName">The name of the channel (pipeline configuration) to use with the client</param>
    /// <param name="configure">A delegate to configure the client options</param>
    public static IServiceCollection AddChannelsNamedClient( this IServiceCollection services, string clientName, string channelName, Action<ChannelsClientOptions> configure )
    {
        services.TryAddSingleton<IChannelsClientFactory, ChannelsClientFactory>();

        /*
        this binds the channel name (pipeline configuration) to the client name
        */
        services.Configure<ChannelName>( clientName, options =>
        {
            options.Name = channelName;
        } );

        services.Configure( clientName, configure );

        return services;
    }

    /// <summary>
    /// Adds a channels client to the service collection
    /// </summary>
    /// <param name="clientName">The name of the client</param>
    /// <param name="channelName">The name of the channel (pipeline configuration) to use with the client</param>
    /// <param name="hostUrl">The URL of the service host</param>
    /// <exception cref="ArgumentException">Thrown when the host URL is not a valid URI</exception>
    /// <exception cref="NotSupportedException">Thrown when the transport mode is not supported</exception>
    public static IServiceCollection AddChannelsNamedClient( this IServiceCollection services, string clientName, string channelName, string hostUrl )
    {
        if ( !Uri.IsWellFormedUriString( hostUrl, UriKind.Absolute ) )
        {
            throw new ArgumentException( "The host URL is not a valid URI.", nameof( hostUrl ) );
        }

        var uri = new Uri( hostUrl );

        /*
        only tcp or udp are supported
        */
        var transportMode = uri.Scheme switch
        {
            "tcp" => ChannelTransportMode.Tcp,
            "udp" => ChannelTransportMode.Udp,
            _ => throw new NotSupportedException( $"Transport mode {uri.Scheme} is not supported." )
        };

        return services.AddChannelsNamedClient( clientName, channelName, options =>
        {
            options.Host = uri.Host;
            options.Port = uri.Port;
            options.TransportMode = transportMode;
        } );
    }

    /// <summary>
    /// Adds a default channels client to the service collection
    /// </summary>
    /// <param name="channelName">The name of the channel (pipeline configuration) to use with the client</param>
    /// <param name="configure">A delegate to configure the client options</param>
    public static IServiceCollection AddChannelsClient( this IServiceCollection services, string channelName, Action<ChannelsClientOptions> configure )
        => services.AddChannelsNamedClient( ChannelsClientFactory.DefaultClientName, channelName, configure );

    /// <summary>
    /// Adds a default channels client to the service collection and binds it to the default channel (pipeline configuration)
    /// </summary>
    /// <param name="configure">A delegate to configure the client options</param>
    public static IServiceCollection AddChannelsClient( this IServiceCollection services, Action<ChannelsClientOptions> configure )
        => services.AddChannelsNamedClient( ChannelsClientFactory.DefaultClientName, ChannelBuilder.DefaultChannelName, configure );

    /// <summary>
    /// Adds a default channels client to the service collection
    /// </summary>
    /// <param name="channelName">The name of the channel (pipeline configuration) to use with the client</param>
    /// <param name="hostUrl">The URL of the service host</param>
    public static IServiceCollection AddChannelsClient( this IServiceCollection services, string channelName, string hostUrl )
        => services.AddChannelsNamedClient( ChannelsClientFactory.DefaultClientName, channelName, hostUrl );

    /// <summary>
    /// Adds a default channels client to the service collection and binds it to the default channel (pipeline configuration)
    /// </summary>
    /// <param name="hostUrl">The URL of the service host</param>
    public static IServiceCollection AddChannelsClient( this IServiceCollection services, string hostUrl )
        => services.AddChannelsNamedClient( ChannelsClientFactory.DefaultClientName, ChannelBuilder.DefaultChannelName, hostUrl );
}
