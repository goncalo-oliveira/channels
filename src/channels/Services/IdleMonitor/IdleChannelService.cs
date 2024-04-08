using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Faactory.Channels;

internal sealed class IdleChannelService : ChannelService
{
    private readonly ILogger logger;
    private readonly IdleDetectionMode detectionMode;
    private readonly TimeSpan timeout;

    public IdleChannelService( 
          ILoggerFactory loggerFactory
        , IOptions<IdleChannelServiceOptions> optionsAccessor )
    {
        logger = loggerFactory.CreateLogger<IdleChannelService>();

        var options = optionsAccessor.Value;

        detectionMode = options.DetectionMode;
        timeout = options.Timeout;
    }

    public override async Task StartAsync( IChannel channel, CancellationToken cancellationToken )
    {
        logger.LogDebug( "Starting..." );

        if ( detectionMode == IdleDetectionMode.None )
        {
            // not enabled
            logger.LogInformation( "Canceled; detection mode is set to 'None'." );

            return;
        }

        if ( ( timeout == TimeSpan.Zero ) && ( detectionMode != IdleDetectionMode.Auto ) )
        {
            // no hard timeout is only allowed with Auto mode
            logger.LogWarning( "Canceled; no hard timeout option requires 'Auto' detection mode." );

            return;
        }

        await base.StartAsync( channel, cancellationToken );

        logger.LogInformation( "Started." );
    }

    public override async Task StopAsync( CancellationToken cancellationToken )
    {
        logger.LogDebug( "Stopping..." );

        await base.StopAsync( cancellationToken );

        logger.LogInformation( "Stopped." );
    }

    protected override async Task ExecuteAsync( CancellationToken cancellationToken )
    {
        var interval = ( detectionMode == IdleDetectionMode.Auto )
            ? TimeSpan.FromSeconds( 5 )
            : timeout;

        var timer = new PeriodicTimer( interval );

        while ( !cancellationToken.IsCancellationRequested && await timer.WaitForNextTickAsync( cancellationToken ) )
        {
            await CheckIdleStateAsync()
                .ConfigureAwait( false );
        }
    }

    private bool IsChannelSocketConnected()
    {
        if ( Channel is Sockets.ConnectedSocket socket)
        {
            return socket.IsConnected();
        }

        // since we can't ask the socket, we assume it is
        return ( true );
    }

    private async Task CheckIdleStateAsync()
    {
        if ( Channel == null )
        {
            // not supposed to happen, but to be on the safe side...
            return;
        }

        // attempt to poll channel socket if auto mode is being used
        if ( ( detectionMode == IdleDetectionMode.Auto ) && !IsChannelSocketConnected() )
        {
            logger.LogWarning( "Channel doesn't seem to be active anymore. Closing..." );

            // await StopAsync( CancellationToken.None )
            //     .ConfigureAwait( false );

            await Channel.CloseAsync()
                .ConfigureAwait( false );
        }

        if ( timeout == TimeSpan.Zero )
        {
            // no hard timeout
            // this is possible only with Auto mode
            return;
        }

        var lastReceived = Channel.LastReceived ?? Channel.Created;
        var lastSent = Channel.LastSent ?? Channel.Created;

        var utcNow = DateTimeOffset.UtcNow;
        var idleReceived = utcNow - lastReceived;
        var idleSent = utcNow - lastSent;
        var idleMin = TimeSpan.FromSeconds( Math.Min( idleReceived.TotalSeconds, idleSent.TotalSeconds ) );

        bool isIdle;
        switch ( detectionMode )
        {
            case IdleDetectionMode.Read:
            {
                isIdle = ( idleReceived > timeout );
                break;
            }

            case IdleDetectionMode.Write:
            {
                isIdle = ( idleSent > timeout );
                break;
            }

            default:
            {
                isIdle = ( idleMin > timeout );
                break;
            }
        }

        if ( isIdle )
        {
            logger.LogWarning(
                "Channel has been idle for more than {seconds} seconds. Closing...",
                (int)timeout.TotalSeconds
            );

            // await StopAsync( CancellationToken.None )
            //     .ConfigureAwait( false );

            await Channel.CloseAsync()
                .ConfigureAwait( false );
        }
    }
}
