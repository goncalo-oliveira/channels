using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Faactory.Channels.Buffers;
using Faactory.Channels.Adapters;
using Faactory.Channels.Handlers;
using Faactory.Channels.Sockets;
using Faactory.Collections;

namespace Faactory.Channels;

public abstract class Channel : ConnectedSocket, IChannel
{
    protected readonly ILogger logger;
    private readonly IDisposable loggerScope;
    private readonly IIdleChannelMonitor? idleMonitor;

    public Channel( ILoggerFactory loggerFactory
        , Socket socket
        , IIdleChannelMonitor? idleChannelMonitor)
        : base( loggerFactory, socket )
    {
        logger = loggerFactory.CreateLogger<IChannel>();
        loggerScope = logger.BeginScope( $"channel-{Id.Substring( 0, 6 )}" );

        Input = new ChannelPipeline( loggerFactory, Array.Empty<IChannelAdapter>(), Array.Empty<IChannelHandler>() );
        Output = new ChannelPipeline( loggerFactory, new IChannelAdapter[]
        {
            new OutputChannelAdapter( loggerFactory )
        }, null );

        idleMonitor = idleChannelMonitor;
        idleMonitor?.Start( this );
    }

    public IByteBuffer Buffer { get; private set; } = new WritableByteBuffer();

    public DateTimeOffset Created { get; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastReceived { get; private set; }
    public DateTimeOffset? LastSent { get; private set; }

    public IMetadata Data { get; } = new Metadata();

    public IChannelPipeline Input { get; protected set; }
    public IChannelPipeline Output { get; protected set; }

    public abstract Task CloseAsync();

    public virtual Task WriteAsync( object data )
    {
        logger.LogDebug( "Executing output pipeline..." );

        Task.Run( () => Output.ExecuteAsync( this, data ) )
            .ConfigureAwait( false )
            .GetAwaiter()
            .GetResult();

        return Task.CompletedTask;
    }

    public virtual void Dispose()
    {
        idleMonitor?.Dispose();

        Input.Dispose();
        Output.Dispose();

        Socket.Dispose();

        logger.LogDebug( "Disposed." );
        loggerScope.Dispose();
    }

    protected override void OnDataReceived( byte[] data )
    {
        LastReceived = DateTimeOffset.UtcNow;

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
    }

    protected override void OnDisconnected()
    {
        logger.LogInformation( "Closed." );

        Dispose();
    }
}
