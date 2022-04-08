using Microsoft.Extensions.DependencyInjection;
using Faactory.Channels.Adapters;
using Faactory.Channels.Handlers;

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

    public IChannelBuilder AddInputHandler<THandler>() where THandler : class, IChannelHandler
    {
        Services.AddTransient<IChannelHandler, THandler>();

        return ( this );
    }
}
