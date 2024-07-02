using Faactory.Channels;

namespace Microsoft.Extensions.DependencyInjection;

public static class ChannelsBuilderServiceExtensions
{
    /// <summary>
    /// Adds a named channel builder to the service collection.
    /// </summary>
    /// <returns>A builder instance that allows adding named channels.</returns>
    public static INamedChannelBuilder AddChannels( this IServiceCollection services )
        => new NamedChannelBuilder( services );

    /// <summary>
    /// Adds a default channel to the service collection.
    /// </summary>
    /// <param name="configure">A delegate that configures the channel options.</param>
    /// <returns></returns>
    public static IServiceCollection AddChannels( this IServiceCollection services, Action<IChannelBuilder> configure )
    {
        var builder = new NamedChannelBuilder( services );

        builder.Add( ChannelBuilder.DefaultChannelName, configure );

        return services;
    }
}
