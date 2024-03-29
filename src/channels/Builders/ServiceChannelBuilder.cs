using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

internal class ServiceChannelBuilder( IServiceCollection services ) : ChannelBuilder( services ), IServiceChannelBuilder
{
    public IServiceChannelBuilder Configure( Action<ServiceChannelOptions> configure )
    {
        Services.Configure( configure );

        return this;
    }
}
