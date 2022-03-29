using Microsoft.Extensions.Logging;

namespace Faactory.Channels;

public interface IChannelContext
{
    ILoggerFactory LoggerFactory { get; }
    IChannel Channel { get; }
    IWritableBuffer Output { get; }
}
