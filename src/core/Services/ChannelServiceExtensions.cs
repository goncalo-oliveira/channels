namespace Faactory.Channels;

public static class ChannelServiceChannelExtensions
{
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
        => channel.GetChannelService( typeof( T ) ) is T service
            ? service
            : throw new InvalidOperationException( $"Channel service {typeof( T ).Name} not found." );
}
