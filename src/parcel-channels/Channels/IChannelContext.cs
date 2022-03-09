using Microsoft.Extensions.Logging;

namespace Parcel.Channels;

public interface IChannelContext
{
    ILoggerFactory LoggerFactory { get; }
    IChannel Channel { get; }
    IWritableBuffer Output { get; }
}
