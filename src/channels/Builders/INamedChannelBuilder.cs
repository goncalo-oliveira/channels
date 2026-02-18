using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

/// <summary>
/// Defines a builder for configuring named channels.
/// </summary>
public interface INamedChannelBuilder
{
    /// <summary>
    /// Gets the service collection to which the named channel is being added.
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    /// Adds a named channel to the service collection.
    /// </summary>
    /// <param name="name">The name of the channel.</param>
    /// <param name="configure">A delegate that configures the channel options.</param>
    INamedChannelBuilder Add( string name, Action<IChannelBuilder> configure );
}
