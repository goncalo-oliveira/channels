using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

internal class ServiceChannelBuilder( IServiceCollection services, string channelName ) : ChannelBuilder<IServiceChannelBuilder>( services, channelName ), IServiceChannelBuilder
{
    public IServiceChannelBuilder Configure( Action<ServiceChannelOptions> configure )
    {
        Services.Configure( Name, configure );

        return Self();
    }

    protected override IServiceChannelBuilder Self() => this;
}
