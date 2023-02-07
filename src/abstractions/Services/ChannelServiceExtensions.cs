namespace Faactory.Channels;

public static class ChannelServiceProviderExtensions
{
    /// <summary>
    /// Gets a specific long-running service owned by the given channel. Returns null if not found.
    /// </summary>
    public static T? GetService<T>( this IChannel channel ) where T : IChannelService
    {
        return channel.Services
            .Where( x => typeof( T ).IsAssignableFrom( x.GetType() ) )
            .Cast<T>()
            .SingleOrDefault();
    }

    /// <summary>
    /// Gets a specific long-running service owned by the given channel.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when required service is not found.</exception>
    public static T GetRequiredService<T>( this IChannel channel ) where T : IChannelService
    {
        return channel.Services
            .Where( x => typeof( T ).IsAssignableFrom( x.GetType() ) )
            .Cast<T>()
            .Single();
    }
}
