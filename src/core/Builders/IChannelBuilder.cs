using Microsoft.Extensions.DependencyInjection;
using Faactory.Channels.Adapters;
using Faactory.Channels.Handlers;

namespace Faactory.Channels;

/// <summary>
/// An interface for configuring a channel
/// </summary>
public interface IChannelBuilder
{
    /// <summary>
    /// Gets the Microsoft.Extensions.DependencyInjection.IServiceCollection where channel services are configured
    /// </summary>
    IServiceCollection Services { get; }
}

public interface IChannelBuilder<TChannelBuilder> : IChannelBuilder where TChannelBuilder : IChannelBuilder<TChannelBuilder>
{
    /// <summary>
    /// Adds a transient service for the channel adapter to the input pipeline
    /// </summary>
    /// <typeparam name="TAdapter">The type of the adapter implementation</typeparam>
    TChannelBuilder AddInputAdapter<TAdapter>() where TAdapter : class, IChannelAdapter, IInputChannelAdapter;

    TChannelBuilder AddInputAdapter( Func<IServiceProvider, IInputChannelAdapter> implementationFactory );

    /// <summary>
    /// Adds a transient service for the channel adapter to the output pipeline
    /// </summary>
    /// <typeparam name="TAdapter">The type of the adapter implementation</typeparam>
    TChannelBuilder AddOutputAdapter<TAdapter>() where TAdapter : class, IChannelAdapter, IOutputChannelAdapter;

    TChannelBuilder AddOutputAdapter( Func<IServiceProvider, IOutputChannelAdapter> implementationFactory );

    /// <summary>
    /// Adds a transient service for the channel handler to the input pipeline
    /// </summary>
    /// <typeparam name="TAdapter">The type of the handler implementation</typeparam>
    TChannelBuilder AddInputHandler<THandler>() where THandler : class, IChannelHandler;

    TChannelBuilder AddInputHandler( Func<IServiceProvider, IChannelHandler> implementationFactory );

    /// <summary>
    /// Adds a long-running service to the channel
    /// </summary>
    /// <typeparam name="TService">The type of the service implementation</typeparam>
    TChannelBuilder AddChannelService<TService>() where TService : class, IChannelService;

    TChannelBuilder AddChannelService( Func<IServiceProvider, IChannelService> implementationFactory );
}
