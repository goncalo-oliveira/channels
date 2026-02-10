namespace Faactory.Channels;

internal sealed class ChannelFactory( IServiceProvider serviceProvider ) : IChannelFactory
{
    public IServiceProvider ChannelServices { get; } = serviceProvider;
}
