using Faactory.Channels.Adapters;
using Faactory.Channels.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

/// <summary>
/// An interface for configuring a channel
/// </summary>
public interface IChannelBuilder
{
    /// <summary>
    /// Gets the name of the channel being configured
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the Microsoft.Extensions.DependencyInjection.IServiceCollection where channel services are configured
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    /// Registers an action used to configure the channel options.
    /// </summary>
    /// <param name="configure">The delegate used to configure the channel options</param>
    IChannelBuilder Configure( Action<ChannelOptions> configure );

    /// <summary>
    /// Registers an action used to configure a particular type of options.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured</typeparam>
    /// <param name="configure">The delegate used to configure the options</param>
    IChannelBuilder Configure<TOptions>( Action<TOptions> configure ) where TOptions : class;

    /// <summary>
    /// Adds a transient service for the channel adapter to the input pipeline
    /// </summary>
    /// <typeparam name="TAdapter">The type of the adapter implementation</typeparam>
    IChannelBuilder AddInputAdapter<TAdapter>() where TAdapter : class, IChannelAdapter, IInputChannelAdapter;

    /// <summary>
    /// Adds a transient service for the channel adapter to the input pipeline
    /// </summary>
    /// <param name="implementationFactory">A factory for creating the adapter instance</param>
    IChannelBuilder AddInputAdapter( Func<IServiceProvider, IInputChannelAdapter> implementationFactory );

    /// <summary>
    /// Adds a transient service for the channel adapter to the output pipeline
    /// </summary>
    /// <typeparam name="TAdapter">The type of the adapter implementation</typeparam>
    IChannelBuilder AddOutputAdapter<TAdapter>() where TAdapter : class, IChannelAdapter, IOutputChannelAdapter;

    /// <summary>
    /// Adds a transient service for the channel adapter to the output pipeline
    /// </summary>
    /// <param name="implementationFactory">A factory for creating the adapter instance</param>
    IChannelBuilder AddOutputAdapter( Func<IServiceProvider, IOutputChannelAdapter> implementationFactory );

    /// <summary>
    /// Adds a transient service for the channel handler to the input pipeline
    /// </summary>
    /// <typeparam name="TAdapter">The type of the handler implementation</typeparam>
    IChannelBuilder AddInputHandler<THandler>() where THandler : class, IChannelHandler;

    /// <summary>
    /// Adds a transient service for the channel handler to the input pipeline
    /// </summary>
    /// <param name="implementationFactory">A factory for creating the handler instance</param>
    IChannelBuilder AddInputHandler( Func<IServiceProvider, IChannelHandler> implementationFactory );

    /// <summary>
    /// Adds a long-running service to the channel
    /// </summary>
    /// <typeparam name="TService">The type of the service implementation</typeparam>
    IChannelBuilder AddChannelService<TService>() where TService : class, IChannelService;

    /// <summary>
    /// Adds a long-running service to the channel
    /// </summary>
    /// <param name="implementationFactory">A factory for creating the service instance</param>
    IChannelBuilder AddChannelService( Func<IServiceProvider, IChannelService> implementationFactory );
}
