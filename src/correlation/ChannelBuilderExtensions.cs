using Faactory.Channels;
using Faactory.Channels.Correlation;

#pragma warning disable IDE0130
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130

/// <summary>
/// Extension methods for <see cref="IChannelBuilder"/> to add correlation support to the channel being built.
/// </summary>
public static class ChannelBuilderCorrelationExtensions
{
    /// <summary>
    /// Adds correlation support to the channel being built.
    /// </summary>
    public static IChannelBuilder AddCorrelation( this IChannelBuilder builder )
    {
        ArgumentNullException.ThrowIfNull( builder );

        builder.Services.AddKeyedScoped<IChannelResponseRegistry, ChannelResponseRegistry>( builder.Name );

        return builder;
    }
}
