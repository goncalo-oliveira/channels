using Microsoft.Extensions.DependencyInjection;
using Faactory.Channels.Adapters;
using Faactory.Channels.Handlers;

namespace Faactory.Channels;

internal abstract class ChannelBuilder<TChannelBuilder>( IServiceCollection services, string channelName ) : IChannelBuilder<TChannelBuilder> where TChannelBuilder : IChannelBuilder<TChannelBuilder>
{
    public string Name { get; } = channelName;
    public IServiceCollection Services { get; } = services;

    public TChannelBuilder AddInputAdapter<TAdapter>() where TAdapter : class, IChannelAdapter, IInputChannelAdapter
    {
        Services.AddKeyedTransient<IInputChannelAdapter, TAdapter>( Name );

        return Self();
    }

    public TChannelBuilder AddInputAdapter( Func<IServiceProvider, IInputChannelAdapter> implementationFactory )
    {
        Services.AddKeyedTransient( Name, ( sp, _ ) =>
        {
            return implementationFactory( sp );
        } );

        return Self();
    }

    public TChannelBuilder AddOutputAdapter<TAdapter>() where TAdapter : class, IChannelAdapter, IOutputChannelAdapter
    {
        Services.AddKeyedTransient<IOutputChannelAdapter, TAdapter>( Name );

        return Self();
    }

    public TChannelBuilder AddOutputAdapter( Func<IServiceProvider, IOutputChannelAdapter> implementationFactory )
    {
        Services.AddKeyedTransient( Name, ( sp, _ ) =>
        {
            return implementationFactory( sp );
        } );

        return Self();
    }

    public TChannelBuilder AddInputHandler<THandler>() where THandler : class, IChannelHandler
    {
        Services.AddKeyedTransient<IChannelHandler, THandler>( Name );

        return Self();
    }

    public TChannelBuilder AddInputHandler( Func<IServiceProvider, IChannelHandler> implementationFactory )
    {
        Services.AddKeyedTransient( Name, ( sp, _ ) =>
        {
            return implementationFactory( sp );
        } );

        return Self();
    }

    public TChannelBuilder AddChannelService<TService>() where TService : class, IChannelService
    {
        Services.AddKeyedScoped<IChannelService, TService>( Name );

        return Self();
    }

    public TChannelBuilder AddChannelService( Func<IServiceProvider, IChannelService> implementationFactory )
    {
        Services.AddKeyedScoped( Name, ( sp, _ ) =>
        {
            return implementationFactory( sp );
        } );

        return Self();
    }

    protected abstract TChannelBuilder Self();
}
