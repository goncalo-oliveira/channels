#pragma warning disable CA1859 // Use concrete types when possible for improved performance

using System.Net.WebSockets;
using Faactory.Channels.Buffers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Faactory.Channels.WebSockets;

internal sealed class WebSocketChannel : Channel, IWebSocketChannel
{
    private readonly ILogger logger;
    private readonly IDisposable? loggerScope;
    private readonly Task initializeTask;
    private readonly Task monitorTask;
    private readonly Task receiveTask;
    private readonly CancellationTokenSource cts = new();

    internal WebSocketChannel( IServiceScope serviceScope, WebSocket socket, ChannelOptions options, IChannelPipeline inputPipeline, IChannelPipeline outputPipeline, IEnumerable<IChannelService>? channelServices = null )
        : base( serviceScope )
    {
        logger = serviceScope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<IChannel>();

        loggerScope = logger.BeginScope( $"ws-{Id[..7]}" );

        Buffer = new WritableByteBuffer( options.BufferEndianness );
        Timeout = options.IdleTimeout;
        WebSocket = socket;
        Input = inputPipeline;
        Output = outputPipeline;

        if ( channelServices != null )
        {
            Services = channelServices;
        }

        initializeTask = base.InitializeAsync( cts.Token );
        monitorTask = MonitorAsync( cts.Token );
        receiveTask = ReceiveAsync( cts.Token );
    }

    private IByteBuffer TextBuffer { get; set; } = new WritableByteBuffer();

    public WebSocket WebSocket { get; }

    public override async Task CloseAsync()
    {
        /*
        Cancel the monitor and receive tasks.
        */
        try
        {
            cts.Cancel();
        }
        catch { }

        /*
        If the WebSocket is open or connecting, close it.
        */
        if ( WebSocket.State == WebSocketState.Open || WebSocket.State == WebSocketState.Connecting )
        {
            await CloseWebSocket();
        }

        /*
        If channel is already closed, return.
        */
        if ( IsClosed )
        {
            return;
        }

        // flag the channel as closed
        IsClosed = true;

        logger.LogInformation( "Channel closed." );

        // notify the channel is closed
        try
        {
            this.NotifyChannelClosed();
        }
        catch ( Exception )
        { }

        // stop all channel services
        await StopServicesAsync()
            .ConfigureAwait( false );
    }

    private async Task CloseWebSocket()
    {
        try
        {
            await WebSocket.CloseAsync(
                closeStatus: WebSocketCloseStatus.NormalClosure,
                statusDescription: null,
                cancellationToken: CancellationToken.None
            );

            /*
            Wait for a response from the server before closing the connection.
            This ensures the server has received the close message and
            a graceful shutdown is performed.
            */
            // if ( waitForResponse )
            // {
            //     await WaitForClosureAsync( cancellationToken );
            // }
        }
        catch ( Exception )
        {
            /*
            This can happen if the server closes the connection abruptly.

            GitHub issue: https://github.com/dotnet/runtime/issues/48246

            This happens because the State is NOT updated and thus the close operation fails.

            Fun fact: this issue is open since 2021. Most likely a fix will never be released.
            */
        }
    }

    public override void Dispose()
    {
        try
        {
            cts.Cancel();
        }
        catch { }

        initializeTask.Dispose();
        monitorTask.Dispose();
        receiveTask.Dispose();

        WebSocket.Dispose();

        logger.LogDebug( "Disposed." );

        loggerScope?.Dispose();

        base.Dispose(); // can't forget to dispose the base class
    }

    protected override Task InitializeAsync( CancellationToken cancellationToken ) => initializeTask;

    internal async Task WriteMessageAsync( WebSocketMessage message )
    {
        try
        {
            await WebSocket.SendAsync(
                message.Data.ToArray(),
                message.Type,
                endOfMessage: message.EndOfMessage,
                cancellationToken: CancellationToken.None
            );

            LastSent = DateTimeOffset.UtcNow;

            this.NotifyDataSent( message.Data.Length );
        }
        catch ( WebSocketException ex )
        {
            logger.LogError( "Failed to send data. {Error}", ex.Message );

            await CloseAsync();
        }
    }

    public override Task WriteRawBytesAsync( byte[] data )
    {
        return WriteMessageAsync( new WebSocketMessage
        {
            Data = new WrappedByteBuffer( data )
        } );
    }

    public async Task WaitAsync( CancellationToken cancellationToken )
    {
        var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            [
                cts.Token,
                cancellationToken
            ]
        );

        while ( !tokenSource.IsCancellationRequested )
        {
            try
            {
                await Task.Delay( 1000, tokenSource.Token );
            }
            catch ( TaskCanceledException )
            {
                break;
            }
        }
    }

    private async Task ReceiveAsync( CancellationToken cancellationToken )
    {
        var buffer = new byte[2048];

        while ( !cancellationToken.IsCancellationRequested )
        {
            try
            {
                var result = await WebSocket.ReceiveAsync(
                    new ArraySegment<byte>( buffer ),
                    cancellationToken
                );

                LastReceived = DateTimeOffset.UtcNow;

                if ( result.MessageType == WebSocketMessageType.Close )
                {
                    logger.LogDebug( "Received close message." );

                    await CloseAsync();

                    break;
                }

                this.NotifyDataReceived( buffer[..result.Count] );

                /*
                Unlike TCP and UDP channels, data received from a WebSocket
                connection is not a continuous stream of bytes. Instead, it's
                a message-based protocol. Each message can be fragmented
                and the EndOfMessage property indicates if the message is
                complete or if more fragments are expected. Messages can
                also be binary or text-based.

                Fragmented messages are first reassembled into a single
                buffer before being passed to the input pipeline.

                A separate buffer is used to store text messages. This is
                to ensure that text messages and binary messages are not
                mixed together.
                */

                WebSocketMessage? message = null;

                if ( result.MessageType == WebSocketMessageType.Text )
                {
                    // reassemble fragmented text messages
                    TextBuffer.WriteBytes( buffer, 0, result.Count );

                    /*
                    If the message is complete, create a new WebSocketMessage.
                    This also clears the text buffer.
                    */
                    if ( result.EndOfMessage )
                    {
                        message = new WebSocketMessage
                        {
                            Type = WebSocketMessageType.Text,
                            Data = TextBuffer.MakeReadOnly()
                        };

                        TextBuffer.DiscardAll();
                    }
                }
                else if ( result.MessageType == WebSocketMessageType.Binary )
                {
                    // reassemble fragmented binary messages
                    Buffer.WriteBytes( buffer, 0, result.Count );

                    /*
                    If the message is complete, create a new WebSocketMessage.
                    This also clears the binary buffer.
                    */
                    if ( result.EndOfMessage )
                    {
                        message = new WebSocketMessage
                        {
                            Type = WebSocketMessageType.Binary,
                            Data = Buffer.MakeReadOnly()
                        };

                        Buffer.DiscardAll();
                    }
                }

                if ( message == null )
                {
                    continue;
                }

                /*
                Messages are passed to the input pipeline for processing.
                These messages are not raw bytes but a WebSocketMessage.

                Messages delivered to the input pipeline are always complete (EndOfMessage = true).
                */
                logger.LogDebug( "Executing input pipeline..." );

                await Input.ExecuteAsync( this, message )
                    .ConfigureAwait( false );
            }
            catch ( OperationCanceledException )
            {
                break;
            }
            catch ( WebSocketException ex )
            {
                logger.LogError( "WebSocket error. {Error}", ex.Message );

                /*
                TODO: we're currently closing the connection on any exception.
                This is not ideal and should be handled differently. The
                connection should only be closed if the exception suggests
                the connection is no longer valid (if that's possible).
                */

                await CloseAsync();
            }
        }

        logger.LogDebug( "Receive task canceled." );
    }

    private async Task MonitorAsync( CancellationToken cancellationToken )
    {
        while ( !cancellationToken.IsCancellationRequested )
        {
            try
            {
                if ( WebSocket.State == WebSocketState.None || WebSocket.State == WebSocketState.Closed || WebSocket.State == WebSocketState.Aborted )
                {
                    logger.LogWarning( "Connection lost." );

                    await CloseAsync();
                }

                await Task.Delay( TimeSpan.FromSeconds( 5 ), cancellationToken );            
            }
            catch ( OperationCanceledException )
            {
                break;
            }
        }

        logger.LogDebug( "Monitor task canceled." );
    }
}

#pragma warning restore CA1859 // Use concrete types when possible for improved performance
