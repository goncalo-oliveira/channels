using Faactory.Channels;

namespace Microsoft.Extensions.DependencyInjection;

public static class IdleChannelServiceChannelBuilderExtensions
{
    /// <summary>
    /// Adds idle channel service
    /// </summary>
    public static IChannelBuilder AddIdleChannelService<TChannelBuilder>( this IChannelBuilder<TChannelBuilder> channel, Action<IdleChannelServiceOptions>? configure = null ) where TChannelBuilder : IChannelBuilder<TChannelBuilder>
    {
        channel.AddChannelService<IdleChannelService>();

        if ( configure != null )
        {
            channel.Services.Configure( configure );
        }

        return ( channel );
    }
}
