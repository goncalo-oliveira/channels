using Faactory.Channels.Buffers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Faactory.Channels.Adapters;

/// <summary>
/// This adapter ensures the length of the input buffer doesn't exceed a maximum value. Configure with BufferLengthAdapterOptions.
/// </summary>
public sealed class BufferLengthAdapter : ChannelAdapter<IReadableByteBuffer>, IInputChannelAdapter
{
    private readonly int maxBufferSize;
    private readonly bool closeChannel;
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the BufferLengthAdapter class.
    /// </summary>
    public BufferLengthAdapter( ILoggerFactory loggerFactory, IOptions<BufferLengthAdapterOptions> optionsAccessor )
    {
        logger = loggerFactory.CreateLogger<BufferLengthAdapter>();

        var options = optionsAccessor.Value;

        maxBufferSize = options.MaxLength;
        closeChannel = options.CloseChannel;
    }
    
    /// <summary>
    /// Executes the adapter logic. If the length of the input buffer exceeds the maximum buffer size, it either closes the channel or discards the data based on the configuration. Otherwise, it forwards the data to the next adapter in the pipeline.
    /// </summary>
    public override Task ExecuteAsync( IAdapterContext context, IReadableByteBuffer data, CancellationToken cancellationToken )
    {
        if ( data.Length > maxBufferSize )
        {
            // maximum buffer size was reached
            // this usually means we are accumulating data and that
            // 1. we are not properly adapting/handling it
            // 2. the data being sent doesn't have the expected structure/format
            // in these situations we either close the channel or discard the data
            var maxSizeMB = maxBufferSize / 1024.0 / 1024.0;

            logger.LogWarning( "IReadableByteBuffer length has exceeded {maxSizeMB:f1} MB.", maxSizeMB );

            if ( closeChannel )
            {
                return context.Channel.CloseAsync();
            }

            return Task.CompletedTask;
        }

        context.Forward( data );

        return Task.CompletedTask;
    }
}
