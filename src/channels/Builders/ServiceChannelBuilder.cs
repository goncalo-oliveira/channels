using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

internal class ServiceChannelBuilder : ChannelBuilder, IServiceChannelBuilder
{
    public ServiceChannelBuilder( IServiceCollection services )
        : base( services )
    {}

    public IServiceChannelBuilder Configure( Action<ServiceChannelOptions> configure )
    {
        Services.Configure( configure );

        return ( this );
    }
}
