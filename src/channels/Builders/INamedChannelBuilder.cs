using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

/*
the main interface for configuring channels
*/

public interface INamedChannelBuilder
{
    IServiceCollection Services { get; }

    /// <summary>
    /// Adds a named channel to the service collection.
    /// </summary>
    /// <param name="name">The name of the channel.</param>
    /// <param name="configure">A delegate that configures the channel options.</param>
    /// <returns></returns>
    INamedChannelBuilder Add( string name, Action<IChannelBuilder> configure );
}
