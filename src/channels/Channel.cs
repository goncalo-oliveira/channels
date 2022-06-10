using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Faactory.Channels.Buffers;
using Faactory.Channels.Adapters;
using Faactory.Channels.Handlers;
using Faactory.Channels.Sockets;
using Faactory.Collections;
using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

public abstract class Channel : ConnectedSocket, IChannel
{
    protected readonly ILogger logger;
    private readonly IDisposable loggerScope;
    private readonly IIdleChannelMonitor? idleMonitor;
    private readonly IServiceScope channelScope;

    public Channel( IServiceScope serviceScope
        , ILoggerFactory loggerFactory
        , Socket socket
        , IIdleChannelMonitor? idleChannelMonitor
        , Buffers.Endianness bufferEndianness )
        : base( loggerFactory, socket )
    {
        logger = loggerFactory.CreateLogger<IChannel>();
        loggerScope = logger.BeginScope( $"channel-{Id.Substring( 0, 6 )}" );

        Input = new ChannelPipeline( loggerFactory, Array.Empty<IChannelAdapter>(), Array.Empty<IChannelHandler>() );
        Output = new ChannelPipeline( loggerFactory, Array.Empty<IChannelAdapter>(), new IChannelHandler[]
        {
            new OutputChannelHandler( loggerFactory )
        } );

        Buffer = new WritableByteBuffer( bufferEndianness );

        idleMonitor = idleChannelMonitor;
        idleMonitor?.Start( this );

        channelScope = serviceScope;

        // notify channel created
        this.NotifyChannelCreated();
    }

    internal IServiceProvider ServiceProvider => channelScope.ServiceProvider;
    internal IChannelInfo Info => new ChannelInfo( this );

    public IByteBuffer Buffer { get; private set; }

    public DateTimeOffset Created { get; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastReceived { get; private set; }
    public DateTimeOffset? LastSent { get; private set; }

    public IMetadata Data { get; } = new Metadata();

    public IChannelPipeline Input { get; protected set; }
    public IChannelPipeline Output { get; protected set; }

    public abstract Task CloseAsync();

    public virtual async Task WriteAsync( object data )
    {
        logger.LogDebug( "Executing output pipeline..." );

        await Output.ExecuteAsync( this, data )
            .ConfigureAwait( false );
        // Task.Run( () => Output.ExecuteAsync( this, data ) )
        //     .ConfigureAwait( false )
        //     .GetAwaiter()
        //     .GetResult();

        //return Task.CompletedTask;
    }

    public virtual void Dispose()
    {
        idleMonitor?.Dispose();

        Input.Dispose();
        Output.Dispose();

        Socket.Dispose();

        logger.LogDebug( "Disposed." );
        loggerScope.Dispose();
        channelScope.Dispose();
    }

    protected override void OnDataReceived( byte[] data )
    {
        LastReceived = DateTimeOffset.UtcNow;

        this.NotifyDataReceived( data );

        Buffer.WriteBytes( data, 0, data.Length );

        var pipelineBuffer = Buffer.MakeReadOnly();

        logger.LogDebug( "Executing input pipeline..." );

        Task.Run( () => Input.ExecuteAsync( this, pipelineBuffer ) )
            .ConfigureAwait( false )
            .GetAwaiter()
            .GetResult();

        pipelineBuffer.DiscardReadBytes();

        Buffer = pipelineBuffer.MakeWritable();

        if ( Buffer.Length > 0 )
        {
            logger.LogDebug( $"Remaining buffer length: {Buffer.Length} byte(s)." );
        }
    }

    protected override void OnDataSent( int bytesSent )
    {
        LastSent = DateTimeOffset.UtcNow;

        this.NotifyDataSent( bytesSent );
    }

    protected override void OnDisconnected()
    {
        logger.LogInformation( "Closed." );

        this.NotifyChannelClosed();

        Dispose();
    }
}
