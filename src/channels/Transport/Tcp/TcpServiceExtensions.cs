using Faactory.Channels;
using Faactory.Channels.Tcp;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class ChannelsTcpServiceExtensions
{
    /// <summary>
    /// Adds a TCP channel listener to the service collection using the default channel pipeline.
    /// </summary>
    /// <param name="configure">A delegate that configures the TCP channel listener options.</param>
    public static IServiceCollection AddTcpChannelListener( this IServiceCollection services, Action<TcpChannelListenerOptions>? configure = null )
        => services.AddTcpChannelListener( ChannelBuilder.DefaultChannelName, configure );

    /// <summary>
    /// Adds a TCP channel listener to the service collection using the default channel pipeline.
    /// </summary>
    /// <param name="port">The port to listen on.</param>
    public static IServiceCollection AddTcpChannelListener( this IServiceCollection services, int port )
        => services.AddTcpChannelListener( ChannelBuilder.DefaultChannelName, port );

    /// <summary>
    /// Adds a TCP channel listener to the service collection.
    /// </summary>
    /// <param name="channelName">The name of the channel pipeline to use.</param>
    /// <param name="configure">A delegate that configures the TCP channel listener options.</param>
    public static IServiceCollection AddTcpChannelListener( this IServiceCollection services, string channelName, Action<TcpChannelListenerOptions>? configure = null )
    {
        services.AddSingleton<IHostedService>( sp =>
        {
            var options = new TcpChannelListenerOptions();

            configure?.Invoke( options );

            return new TcpListener( sp, channelName, options );
        } );

        return services;
    }

    /// <summary>
    /// Adds a TCP channel listener to the service collection.
    /// </summary>
    /// <param name="channelName">The name of the channel pipeline to use.</param>
    /// <param name="port">The port to listen on.</param>
    public static IServiceCollection AddTcpChannelListener( this IServiceCollection services, string channelName, int port )
        => services.AddTcpChannelListener( channelName, options =>
        {
            options.Port = port;
        } );
}
