using Microsoft.Extensions.DependencyInjection;
using Faactory.Channels.Adapters;
using Faactory.Channels.Handlers;

namespace Faactory.Channels;

internal static class ServiceProviderPipelineExtensions
{
    public static IChannelAdapter[] GetAdapters<TPipeline>( this IServiceProvider serviceProvider )
        => serviceProvider.GetServices<TPipeline>()
            .Cast<IChannelAdapter>()
            .ToArray();

    public static IChannelHandler[] GetHandlers( this IServiceProvider serviceProvider )
        => serviceProvider.GetServices<IChannelHandler>()
            .ToArray();
}
