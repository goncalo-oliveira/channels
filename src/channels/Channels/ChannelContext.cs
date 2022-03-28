using Microsoft.Extensions.Logging;

namespace Faactory.Channels;

internal class ChannelContext : IChannelContext
{
    public ChannelContext( ILoggerFactory loggerFactory, IChannel channel )
    {
        LoggerFactory = loggerFactory;
        Channel = channel;
        Output = new WritableBuffer();
    }

    public ILoggerFactory LoggerFactory { get; }

    public IChannel Channel { get; }

    public IWritableBuffer Output { get; }
}
