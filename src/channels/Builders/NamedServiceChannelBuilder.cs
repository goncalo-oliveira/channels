using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

public interface INamedServiceChannelBuilder
{
    IServiceCollection Services { get; }

    INamedServiceChannelBuilder AddChannel( string name, Action<IServiceChannelBuilder> configure );
}

internal sealed class NamedServiceChannelBuilder( IServiceCollection services ) : INamedServiceChannelBuilder
{
    public IServiceCollection Services { get; } = services;

    public INamedServiceChannelBuilder AddChannel( string name, Action<IServiceChannelBuilder> configure )
    {
        var channelBuilder = new ServiceChannelBuilder( Services, name );

        configure?.Invoke( channelBuilder );

        return this;
    }
}
