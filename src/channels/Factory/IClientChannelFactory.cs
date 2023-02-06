namespace Faactory.Channels;

/// <summary>
/// A factory for creating client channel instances
/// </summary>
public interface IClientChannelFactory
{
    /// <summary>
    /// Creates a new client channel instance
    /// </summary>
    Task<IChannel> CreateAsync( CancellationToken cancellationToken = default( CancellationToken ) );

    /// <summary>
    /// Creates a new client channel instance using the given named options
    /// </summary>
    Task<IChannel> CreateAsync( string name, CancellationToken cancellationToken = default( CancellationToken ) );
}
