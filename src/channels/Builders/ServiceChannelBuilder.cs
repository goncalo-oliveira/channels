using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

internal class ServiceChannelBuilder( IServiceCollection services ) : ChannelBuilder<IServiceChannelBuilder>( services ), IServiceChannelBuilder
{
    public IServiceChannelBuilder Configure( Action<ServiceChannelOptions> configure )
    {
        Services.Configure( configure );

        return Self();
    }

    protected override IServiceChannelBuilder Self() => this;
}
