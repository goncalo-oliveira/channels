namespace Faactory.Channels;

/// <summary>
/// Extension methods for channel services
/// </summary>
public static class ChannelServiceExtensions
{
    /// <summary>
    /// Gets a channel service of the specified type, or null if not found
    /// </summary>
    /// <param name="channel">The channel to retrieve the service from</param>
    /// <param name="serviceType">The type of the channel service to retrieve</param>
    /// <returns>The channel service instance if found, or null if not found</returns>
    public static IChannelService? GetChannelService( this IChannel channel, Type serviceType )
        => channel.ChannelServices.SingleOrDefault( s => s.GetType() == serviceType );

    /// <summary>
    /// Gets a channel service of the specified type, or null if not found
    /// </summary>
    /// <typeparam name="T">The type of the channel service to retrieve</typeparam>
    /// <param name="channel">The channel to retrieve the service from</param>
    /// <returns>The channel service instance if found, or null if not found</returns>
    public static T? GetChannelService<T>( this IChannel channel ) where T : class, IChannelService
        => channel.GetChannelService( typeof( T ) ) as T;

    /// <summary>
    /// Gets a channel service of the specified type, or throws an exception if not found
    /// </summary>
    /// <typeparam name="T">The type of the channel service to retrieve</typeparam>
    /// <param name="channel">The channel to retrieve the service from</param>
    /// <returns>The channel service instance if found</returns>
    /// <exception cref="InvalidOperationException">Thrown if the channel service is not found</exception>
    public static T GetRequiredChannelService<T>( this IChannel channel ) where T : class, IChannelService
        => channel.GetChannelService<T>() is T service
            ? service
            : throw new InvalidOperationException( $"Channel service {typeof( T ).Name} not found." );
}
