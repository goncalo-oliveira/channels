using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

public static class ChannelBuilderConfigureExtensions
{
    /// <summary>
    /// Registers an action used to configure a particular type of options.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    public static IChannelBuilder Configure<TOptions>( this IChannelBuilder builder, Action<TOptions> configure ) where TOptions : class
    {
        builder.Services.Configure( configure );

        return ( builder );
    }

    /// <summary>
    /// Registers an action used to configure a particular type of options.
    /// </summary>
    /// <param name="name">The name of the options instance</param>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    public static IChannelBuilder Configure<TOptions>( this IChannelBuilder builder, string name, Action<TOptions> configure ) where TOptions : class
    {
        builder.Services.Configure( name, configure );

        return ( builder );
    }
}
