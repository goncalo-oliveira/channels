using System.Text;
using Faactory.Channels.Adapters;
using Faactory.Channels.Buffers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Faactory.Channels.Adapters;

public class BufferLengthAdapterOptions
{
    public BufferLengthAdapterOptions()
    {
        MaxLength = 1024 * 1024;
        CloseChannel = true;
    }

    /// <summary>
    /// The maximum length for the input IByteBuffer. Default is 1 MB.
    /// </summary>
    public int MaxLength { get; set; }

    /// <summary>
    /// If true, the channel is closed when the maximum length is exceeded. Default is true.
    /// </summary>
    public bool CloseChannel { get; set; }
}

/// <summary>
/// This adapter ensures the length of the input buffer doesn't exceed a maximum value. Configure with BufferLengthAdapterOptions.
/// </summary>
public sealed class BufferLengthAdapter : ChannelAdapter<IByteBuffer>
{
    private readonly int maxBufferSize;
    private readonly bool closeChannel;
    private readonly ILogger logger;

    public BufferLengthAdapter( ILoggerFactory loggerFactory, IOptions<BufferLengthAdapterOptions> optionsAccessor )
    {
        logger = loggerFactory.CreateLogger<BufferLengthAdapter>();

        var options = optionsAccessor.Value;

        maxBufferSize = options.MaxLength;
        closeChannel = options.CloseChannel;
    }
    
    public override Task ExecuteAsync( IAdapterContext context, IByteBuffer data )
    {
        if ( data.Length > maxBufferSize )
        {
            // maximum buffer size was reached
            // this usually means we are accumulating data and that
            // 1. we are not properly adapting/handling it
            // 2. the data being sent doesn't have the expected structure/format
            // in these situations we either close ther channel or discard the data
            var maxSizeMB = maxBufferSize / 1024.0 / 1024.0;

            logger.LogWarning( $"IByteBuffer length has exceeded {maxSizeMB:f1} MB." );

            if ( closeChannel )
            {
                return context.Channel.CloseAsync();
            }

            data.DiscardAll();
        }

        return Task.CompletedTask;
    }
}
