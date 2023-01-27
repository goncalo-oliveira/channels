using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

public static class ChannelServicesExtensions
{
    internal static Task StartChannelServicesAsync( this Channel channel, CancellationToken cancellationToken = default )
        => channel.GetServices()
            .InvokeAllAsync( x => x.StartAsync( channel, cancellationToken ) );

    internal static Task StopChannelServicesAsync( this Channel channel, CancellationToken cancellationToken = default )
        => channel.GetServices()
            .InvokeAllAsync( x => x.StopAsync( cancellationToken ) );

    internal static IEnumerable<IChannelService> GetServices( this Channel channel )
        => channel.ServiceProvider.GetServices<IChannelService>();

    public static IEnumerable<IChannelService> GetServices( this IChannel channel )
    {
        if ( !( channel is Channel ) )
        {
            return Enumerable.Empty<IChannelService>();
        }

        return GetServices( (Channel)channel );
    }

    public static T? GetService<T>( this IChannel channel ) where T : IChannelService
    {
        if ( !( channel is Channel ) )
        {
            return default;
        }

        return GetServices( (Channel)channel )
            .Where( x => typeof( T ).IsAssignableFrom( x.GetType() ) )
            .Cast<T>()
            .SingleOrDefault();
    }
}
