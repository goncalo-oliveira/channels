using Faactory.Channels.Correlation;

#pragma warning disable IDE0130
namespace Faactory.Channels;
#pragma warning restore IDE0130

/// <summary>
/// Extension methods for <see cref="IChannel"/> to access the channel response registry.
/// </summary>
public static class ChannelRegistryExtensions
{
    /// <summary>
    /// Gets the <see cref="IChannelResponseRegistry"/> bound to the channel.
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <returns>An instance of <see cref="IChannelResponseRegistry"/> that shares the same scope as the channel.</returns>
    public static IChannelResponseRegistry GetChannelResponseRegistry( this IChannel channel )
    {
        ArgumentNullException.ThrowIfNull( channel );

        return channel.GetRequiredChannelService<CorrelationChannelService>().Registry;
    }
}
