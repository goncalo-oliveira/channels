using Faactory.Channels;
using Faactory.Channels.Buffers;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for adding channels services to the service collection.
/// </summary>
public static class ChannelsBuilderServiceExtensions
{
    /// <summary>
    /// Adds a named channel builder to the service collection.
    /// </summary>
    /// <returns>A builder instance that allows adding named channels.</returns>
    public static INamedChannelBuilder AddChannels( this IServiceCollection services )
    {
        AddDependencies( services );

        return new NamedChannelBuilder( services );
    }

    /// <summary>
    /// Adds a default channel to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the channel services to.</param>
    /// <param name="configure">A delegate that configures the channel options.</param>
    /// <returns></returns>
    public static IServiceCollection AddChannels( this IServiceCollection services, Action<IChannelBuilder> configure )
    {
        AddDependencies( services );

        var builder = new NamedChannelBuilder( services );

        builder.Add( ChannelBuilder.DefaultChannelName, configure );

        return services;
    }

    /// <summary>
    /// Registers Channels dependencies to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the dependencies to.</param>
    /// <returns>The updated service collection.</returns>
    private static IServiceCollection AddDependencies( IServiceCollection services )
    {
        /*
        Channel factory is just a placeholder for the channel services provider,
        which is used to create channel instances and resolve channel adapters.

        It also serves as an anchor for extending the factory with different channel types (e.g. Clients, WebSockets, etc.).
        */
        services.TryAddTransient<IChannelFactory, ChannelFactory>();

        /*
        Byte buffer pool is used to manage reusable byte buffers,
        which helps reduce memory allocations and improve performance.
        */
        services.TryAddSingleton<IByteBufferPool, ByteBufferPool>();

        return services;
    }
}
