using Microsoft.Extensions.DependencyInjection;
using Faactory.Channels.Adapters;

namespace Faactory.Channels;

internal class ChannelBuilder : IChannelBuilder
{
    internal ChannelBuilder( IServiceCollection services )
    {
        Services = services;
    }

    public IServiceCollection Services { get; }

    public IChannelBuilder AddInputAdapter<TAdapter>() where TAdapter : class, IChannelAdapter, IInputChannelAdapter
    {
        Services.AddTransient<IInputChannelAdapter, TAdapter>();

        return ( this );
    }

    public IChannelBuilder AddOutputAdapter<TAdapter>() where TAdapter : class, IChannelAdapter, IOutputChannelAdapter
    {
        Services.AddTransient<IOutputChannelAdapter, TAdapter>();

        return ( this );
    }
}
