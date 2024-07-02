using Faactory.Channels;
using Faactory.Channels.Udp;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class ChannelsUdpServiceExtensions
{
    /// <summary>
    /// Adds an UDP channel listener to the service collection using the default channel pipeline.
    /// </summary>
    /// <param name="configure">A delegate that configures the UDP channel listener options.</param>
    public static IServiceCollection AddUdpChannelListener( this IServiceCollection services, Action<UdpChannelListenerOptions>? configure = null )
        => services.AddUdpChannelListener( ChannelBuilder.DefaultChannelName, configure );

    /// <summary>
    /// Adds an UDP channel listener to the service collection using the default channel pipeline.
    /// </summary>
    /// <param name="port">The port to listen on.</param>
    public static IServiceCollection AddUdpChannelListener( this IServiceCollection services, int port )
        => services.AddUdpChannelListener( ChannelBuilder.DefaultChannelName, port );

    /// <summary>
    /// Adds an UDP channel listener to the service collection.
    /// </summary>
    /// <param name="channelName">The name of the channel pipeline to use.</param>
    /// <param name="configure">A delegate that configures the UDP channel listener options.</param>
    public static IServiceCollection AddUdpChannelListener( this IServiceCollection services, string channelName, Action<UdpChannelListenerOptions>? configure = null )
    {
        services.AddSingleton<IHostedService>( sp =>
        {
            var options = new UdpChannelListenerOptions();

            configure?.Invoke( options );

            return new UdpListener( sp, channelName, options );
        } );

        return services;
    }

    /// <summary>
    /// Adds an UDP channel listener to the service collection.
    /// </summary>
    /// <param name="channelName">The name of the channel pipeline to use.</param>
    /// <param name="port">The port to listen on.</param>
    public static IServiceCollection AddUdpChannelListener( this IServiceCollection services, string channelName, int port )
        => services.AddUdpChannelListener( channelName, options =>
        {
            options.Port = port;
        } );
}
