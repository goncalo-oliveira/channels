using Faactory.Channels;

namespace Microsoft.Extensions.DependencyInjection;

public static class IdleChannelServiceChannelBuilderExtensions
{
    /// <summary>
    /// Adds idle channel service
    /// </summary>
    public static IChannelBuilder AddIdleChannelService( this IChannelBuilder channel, Action<IdleChannelServiceOptions>? configure = null )
    {
        channel.AddChannelService<IdleChannelService>();

        if ( configure != null )
        {
            channel.Services.Configure( configure );
        }

        return channel;
    }
}
