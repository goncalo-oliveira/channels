using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels.Client;

public interface INamedClientBuilder
{
    IServiceCollection Services { get; }

    INamedClientBuilder AddChannel( string name, Action<IChannelsClientBuilder> configure );
}

internal sealed class NamedClientBuilder( IServiceCollection services ) : INamedClientBuilder
{
    public IServiceCollection Services { get; } = services;

    public INamedClientBuilder AddChannel( string name, Action<IChannelsClientBuilder> configure )
    {
        var channelBuilder = new ChannelsClientBuilder( Services, name );

        configure?.Invoke( channelBuilder );

        return this;
    }
}
