using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Faactory.Channels;

/// <summary>
/// Extension methods for adding a connection limiter to a channel builder.
/// </summary>
public static class ChannelLimiterBuilderExtensions
{
    /// <summary>
    /// Adds a connection limiter to the channel builder with the specified options.
    /// </summary>
    /// <param name="builder">The channel builder to add the connection limiter to.</param>
    /// <param name="configureOptions">A delegate to configure the channel limiter options.</param>
    /// <returns>The channel builder with the connection limiter added.</returns>
    public static IChannelBuilder AddConnectionLimiter( this IChannelBuilder builder, Action<ChannelLimiterOptions> configureOptions )
    {
        builder.Services.Configure( builder.Name, configureOptions );

        builder.Services.AddSingleton<ChannelLimiter>( sp =>
        {
            var options = sp.GetRequiredService<IOptionsSnapshot<ChannelLimiterOptions>>()
                .Get( builder.Name );

            return new ChannelLimiter( options.ConnectionLimit );
        } );

        builder.Services.AddSingleton<IChannelMonitor>( sp =>
            sp.GetRequiredService<ChannelLimiter>()
        );

        builder.Services.AddSingleton<IChannelLimiter>( sp =>
            sp.GetRequiredService<ChannelLimiter>()
        );

        return builder;
    }
}
