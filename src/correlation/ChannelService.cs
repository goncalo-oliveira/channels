
namespace Faactory.Channels.Correlation;

/// <summary>
/// A channel service that provides scoped access to a correlation registry.
/// </summary>
/// <remarks>
/// This allows you to use the registry within the context of a channel service, when you don't have direct access to the channel's scope.
/// </remarks>
internal sealed class CorrelationChannelService( IChannelResponseRegistry registry ) : IChannelService
{
    /// <summary>
    /// Gets the correlation registry bound to the channel's scope.
    /// </summary>
    public IChannelResponseRegistry Registry => registry;

    public void Dispose()
    { }

    public Task StartAsync( IChannel channel, CancellationToken cancellationToken )
    {
        return Task.CompletedTask;
    }

    public Task StopAsync( CancellationToken cancellationToken )
    {
        return Task.CompletedTask;
    }
}
