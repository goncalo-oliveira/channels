using Microsoft.Extensions.DependencyInjection;
using Faactory.Channels.Adapters;
using Faactory.Channels.Handlers;

namespace Faactory.Channels;

internal abstract class ChannelFactory
{
    private readonly IServiceProvider serviceProvider;

    public ChannelFactory( IServiceProvider serviceProvider )
    {
        this.serviceProvider = serviceProvider;
    }

    protected IEnumerable<IChannelAdapter> GetAdapters<TPipeline>()
        => serviceProvider.GetServices<TPipeline>()
            .Cast<IChannelAdapter>()
            .ToArray();

    protected IEnumerable<IChannelHandler> GetHandlers()
        => serviceProvider.GetServices<IChannelHandler>()
            .ToArray();
}
