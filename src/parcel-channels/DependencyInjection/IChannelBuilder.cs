using Microsoft.Extensions.DependencyInjection;
using Parcel.Channels.Adapters;

namespace Parcel.Channels;

/// <summary>
/// An interface for configuring a channel
/// </summary>
public interface IChannelBuilder
{
    /// <summary>
    /// Gets the Microsoft.Extensions.DependencyInjection.IServiceCollection where channel services are configured
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    /// Adds a transient service for the channel adapter to the input pipeline
    /// </summary>
    /// <typeparam name="TAdapter">The type of the adapter implementation</typeparam>
    IChannelBuilder AddInputAdapter<TAdapter>() where TAdapter : class, IChannelAdapter, IInputChannelAdapter;

    /// <summary>
    /// Adds a transient service for the channel adapter to the output pipeline
    /// </summary>
    /// <typeparam name="TAdapter">The type of the adapter implementation</typeparam>
    IChannelBuilder AddOutputAdapter<TAdapter>() where TAdapter : class, IChannelAdapter, IOutputChannelAdapter;
}
