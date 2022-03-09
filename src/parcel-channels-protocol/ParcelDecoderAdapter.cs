using Microsoft.Extensions.Logging;
using Parcel.Buffers;
using Parcel.Protocol;

namespace Parcel.Channels.Adapters;

/// <summary>
/// Decodes Parcel Messages from an IByteBuffer, forwarding a Message array
/// </summary>
public sealed class ParcelDecoderAdapter : ChannelAdapter<IByteBuffer>
{
    private readonly ILogger logger;

    public ParcelDecoderAdapter( ILoggerFactory loggerFactory )
    {
        logger = loggerFactory.CreateLogger<ParcelDecoderAdapter>();
    }

    public override Task ExecuteAsync( IAdapterContext context, IByteBuffer data )
    {
        var forwardData = new List<Message>();

        while ( data.ReadableBytes > 0 )
        {
            // a packet is at least 11 bytes
            if ( data.Length < 11 )
            {
                logger.LogDebug( "Not enough data. A Parcel packet requires at least 11 bytes." );

                // forward content to next adapter
                context.Forward( data );

                return Task.CompletedTask;
            }

            // check we have the entire packet
            if ( !CanReadPacket( data ) )
            {
                logger.LogDebug( "Not enough data. Don't have the full packet." );

                // forward content to next adapter
                context.Forward( data );

                return Task.CompletedTask;
            }

            var packetMessages = ReadPacket( data );

            forwardData.AddRange( packetMessages );
        }

        // forward messages
        if ( forwardData.Any() )
        {
            context.Forward( forwardData.ToArray() );

            logger.LogDebug( $"Forwarded {forwardData.Count} Parcel message(s)." );
        }

        return Task.CompletedTask;
    }

    private bool CanReadPacket( IByteBuffer data )
    {
        var offset = 0;
        if ( data.GetByte( offset++ ) != Constants.Head )
        {
            throw new FormatException( "Head not found!" );
        }

        var length = data.GetUInt32( offset );

        offset += sizeof( uint );

        // check if we have the entire packet
        return !( data.Length < ( length + 2 + 4 ) );
    }

    private IEnumerable<Message> ReadPacket( IByteBuffer data )
    {
        var offset = 0;
        if ( data.GetByte( offset++ ) != Constants.Head )
        {
            throw new FormatException( "Head not found!" );
        }

        var length = data.GetUInt32( offset );

        offset += sizeof( uint );

        var packetTailOffset = (int)( length + 5 );

        if ( data.GetByte( packetTailOffset ) != Constants.Tail )
        {
            throw new FormatException( "Tail not found!" );
        }

        data.SkipBytes( offset );

        var messages = new List<Message>();

        while ( offset < packetTailOffset )
        {
            var message = data.ReadMessage();

            offset += message.Length;

            messages.Add( message );
        }

        data.ReadByte(); // tail

        return ( messages );
    }
}
