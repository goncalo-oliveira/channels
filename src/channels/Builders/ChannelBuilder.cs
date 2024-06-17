using Microsoft.Extensions.DependencyInjection;
using Faactory.Channels.Adapters;
using Faactory.Channels.Handlers;

namespace Faactory.Channels;

internal abstract class ChannelBuilder<TChannelBuilder> : IChannelBuilder<TChannelBuilder> where TChannelBuilder : IChannelBuilder<TChannelBuilder>
{
    protected ChannelBuilder( IServiceCollection services )
    {
        Services = services;
    }

    public IServiceCollection Services { get; }

    public TChannelBuilder AddInputAdapter<TAdapter>() where TAdapter : class, IChannelAdapter, IInputChannelAdapter
    {
        Services.AddTransient<IInputChannelAdapter, TAdapter>();

        return Self();
    }

    public TChannelBuilder AddInputAdapter( Func<IServiceProvider, IInputChannelAdapter> implementationFactory )
    {
        Services.AddTransient( implementationFactory );

        return Self();
    }

    public TChannelBuilder AddOutputAdapter<TAdapter>() where TAdapter : class, IChannelAdapter, IOutputChannelAdapter
    {
        Services.AddTransient<IOutputChannelAdapter, TAdapter>();

        return Self();
    }

    public TChannelBuilder AddOutputAdapter( Func<IServiceProvider, IOutputChannelAdapter> implementationFactory )
    {
        Services.AddTransient( implementationFactory );

        return Self();
    }

    public TChannelBuilder AddInputHandler<THandler>() where THandler : class, IChannelHandler
    {
        Services.AddTransient<IChannelHandler, THandler>();

        return Self();
    }

    public TChannelBuilder AddInputHandler( Func<IServiceProvider, IChannelHandler> implementationFactory )
    {
        Services.AddTransient( implementationFactory );

        return Self();
    }

    public TChannelBuilder AddChannelService<TService>() where TService : class, IChannelService
    {
        Services.AddScoped<IChannelService, TService>();

        return Self();
    }

    public TChannelBuilder AddChannelService( Func<IServiceProvider, IChannelService> implementationFactory )
    {
        Services.AddScoped( implementationFactory );

        return Self();
    }

    protected abstract TChannelBuilder Self();
}
