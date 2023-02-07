using Faactory.Channels;

namespace Microsoft.Extensions.DependencyInjection;

public static class IdleChannelServiceChannelBuilderExtensions
{
    /// <summary>
    /// Adds idle channel service
    /// </summary>
    public static IChannelBuilder AddIdleChannelService( this IChannelBuilder builder, Action<IdleChannelServiceOptions>? configure = null )
    {
        builder.AddService<IdleChannelService>();

        if ( configure != null )
        {
            builder.Services.Configure( configure );
        }

        return ( builder );
    }
}
