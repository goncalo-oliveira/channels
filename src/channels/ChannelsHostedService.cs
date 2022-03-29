using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Faactory.Channels;

namespace Faactory.Channels.Hosting;

internal sealed class ChannelsHostedService : IHostedService
{
    private readonly ILoggerFactory loggerFactory;
    private readonly ILogger logger;
    private readonly IHostApplicationLifetime appLifetime;
    private readonly int listenPort;
    private readonly int so_backlog;

    private readonly ManualResetEvent allDone = new ManualResetEvent( false );

    private readonly IServiceChannelFactory channelFactory;
    private Socket? listener;

    public ChannelsHostedService( ILoggerFactory loggerFactory
        , IHostApplicationLifetime hostApplicationLifetime
        , IServiceChannelFactory channelFactory
        , IOptions<ServiceChannelOptions> optionsAccessor )
    {
        this.loggerFactory = loggerFactory;
        this.channelFactory = channelFactory;
        logger = loggerFactory.CreateLogger<ChannelsHostedService>();
        appLifetime = hostApplicationLifetime;

        var options = optionsAccessor.Value;

        listenPort = options.Port;
        so_backlog = options.Backlog;
    }

    public Task StartAsync( CancellationToken cancellationToken )
    {
        logger.LogInformation( "Starting service..." );

        appLifetime.ApplicationStarted.Register( OnStarted );
        appLifetime.ApplicationStopping.Register( OnStopping );
        appLifetime.ApplicationStopped.Register( OnStopped );

        return Task.CompletedTask;
    }

    public Task StopAsync( CancellationToken cancellationToken )
    {
        return Task.CompletedTask;
    }

    private void OnStarted()
    {
        logger.LogDebug( $"pid: {System.Diagnostics.Process.GetCurrentProcess().Id}" );

        try
        {
            logger.LogInformation( "Service started." );

            // establish the local endpoint for the socket
            var ipAddress = IPAddress.Any;
            var localEndPoint = new IPEndPoint( ipAddress, listenPort );

            logger.LogDebug( $"Attempting to listen on port {listenPort}..." );

            // create a TCP streaming socket
            listener = new Socket( ipAddress.AddressFamily
                , SocketType.Stream
                , ProtocolType.Tcp );

            listener.Bind( localEndPoint );
            listener.Listen( so_backlog );

            logger.LogInformation( $"Listening on port {listenPort}." );

            while ( !listener.SafeHandle.IsClosed )
            {
                // non-signaled state
                allDone.Reset();

                // Start an asynchronous socket to listen for connections
                listener.BeginAccept( new AsyncCallback( AcceptCallback ), listener );

                // Wait until a connection is made before continuing
                allDone.WaitOne();
            }

            logger.LogDebug( $"No longer listening on port {listenPort}." );
        }
        catch ( Exception ex )
        {
            logger.LogError( ex, $"Failed to start service. {ex.Message}." );

            appLifetime.StopApplication();
        }
    }

    private void OnStopping()
    {
        logger.LogInformation( "Stopping service..." );

        try
        {
            listener?.Close();
        }
        catch ( Exception ex )
        {
            logger.LogError( ex, $"Failed to close listener. {ex.Message}." );
        }
    }

    private void OnStopped()
    {
        logger.LogInformation( "Service stopped." );
    }

    private void AcceptCallback( IAsyncResult ar )
    {
        logger.LogDebug( "Accepting incoming connection..." );

        // Get the socket that handles the client request
        var listener = (Socket)ar.AsyncState!;
        Socket handler;

        // Signal the main thread to continue
        allDone.Set();

        if ( listener.SafeHandle.IsClosed )
        {
            // listener socket is closed
            return;
        }

        try
        {
            handler = listener.EndAccept( ar );
        }
        catch ( Exception ex )
        {
            logger.LogError( ex, $"failed to accept incoming connection. {ex.Message}" );

            return;
        }

        logger.LogInformation( "Accepted incoming connection." );

        var channel = channelFactory.CreateChannel( handler );
    }
}
