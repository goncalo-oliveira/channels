using Faactory.Channels.Adapters;
using Faactory.Channels.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

internal class ChannelBuilder( IServiceCollection services, string channelName ) : IChannelBuilder
{
    public const string DefaultChannelName = "__default";

    /// <summary>
    /// Gets the name of the channel
    /// </summary>
    public string Name { get; } = channelName;
    public IServiceCollection Services { get; } = services;

    public IChannelBuilder Configure( Action<ChannelOptions> configure )
    {
        Services.Configure( Name, configure );

        return this;
    }

    public IChannelBuilder AddInputAdapter<TAdapter>() where TAdapter : class, IChannelAdapter, IInputChannelAdapter
    {
        Services.AddKeyedTransient<IInputChannelAdapter, TAdapter>( Name );

        return this;
    }

    public IChannelBuilder AddInputAdapter( Func<IServiceProvider, IInputChannelAdapter> implementationFactory )
    {
        Services.AddKeyedTransient( Name, ( sp, _ ) =>
        {
            return implementationFactory( sp );
        } );

        return this;
    }

    public IChannelBuilder AddOutputAdapter<TAdapter>() where TAdapter : class, IChannelAdapter, IOutputChannelAdapter
    {
        Services.AddKeyedTransient<IOutputChannelAdapter, TAdapter>( Name );

        return this;
    }

    public IChannelBuilder AddOutputAdapter( Func<IServiceProvider, IOutputChannelAdapter> implementationFactory )
    {
        Services.AddKeyedTransient( Name, ( sp, _ ) =>
        {
            return implementationFactory( sp );
        } );

        return this;
    }

    public IChannelBuilder AddInputHandler<THandler>() where THandler : class, IChannelHandler
    {
        Services.AddKeyedTransient<IChannelHandler, THandler>( Name );

        return this;
    }

    public IChannelBuilder AddInputHandler( Func<IServiceProvider, IChannelHandler> implementationFactory )
    {
        Services.AddKeyedTransient( Name, ( sp, _ ) =>
        {
            return implementationFactory( sp );
        } );

        return this;
    }

    public IChannelBuilder AddChannelService<TService>() where TService : class, IChannelService
    {
        Services.AddKeyedScoped<IChannelService, TService>( Name );

        return this;
    }

    public IChannelBuilder AddChannelService( Func<IServiceProvider, IChannelService> implementationFactory )
    {
        Services.AddKeyedScoped( Name, ( sp, _ ) =>
        {
            return implementationFactory( sp );
        } );

        return this;
    }
}
