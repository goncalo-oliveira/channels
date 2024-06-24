using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

internal sealed class NamedChannelBuilder( IServiceCollection services ) : INamedChannelBuilder
{
    public IServiceCollection Services { get; } = services;

    public INamedChannelBuilder Add( string name, Action<IChannelBuilder> configure )
    {
        var builder = new ChannelBuilder( Services, name );

        configure?.Invoke( builder );

        return this;
    }
}
