using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Parcel.Channels;

internal class IdleChannelMonitor : IIdleChannelMonitor
{
    private readonly ILogger logger;
    private Timer? timer;
    private readonly IdleDetectionMode detectionMode;
    private readonly TimeSpan timeout;

    public IdleChannelMonitor( 
          ILoggerFactory loggerFactory
        , IdleDetectionMode idleDetectionMode
        , TimeSpan idleDetectionTimeout )
    {
        logger = loggerFactory.CreateLogger<IdleChannelMonitor>();

        detectionMode = idleDetectionMode;
        timeout = idleDetectionTimeout;

        if ( detectionMode == IdleDetectionMode.None )
        {
            throw new ArgumentOutOfRangeException( nameof( detectionMode ) );
        }

        if ( ( timeout == TimeSpan.Zero ) && ( detectionMode != IdleDetectionMode.Auto ) )
        {
            // no hard timeout is only allowed with Auto mode
            throw new ArgumentOutOfRangeException( nameof( timeout ) );
        }

        timer = new Timer( IdleDetectionTimeoutCallback
            , null
            , Timeout.Infinite, Timeout.Infinite );
    }

    public void Start( IChannel channel )
    {

        var dueTime = ( detectionMode == IdleDetectionMode.Auto )
            ? TimeSpan.FromSeconds( 5 )
            : timeout;

        var intervalTime = ( detectionMode == IdleDetectionMode.Auto )
            ? dueTime
            : ( timeout / 2 );

        timer = new Timer( IdleDetectionTimeoutCallback
            , channel
            , dueTime, intervalTime );
    }

    public void Stop()
    {
        timer?.Change( Timeout.Infinite, Timeout.Infinite );
        timer?.Dispose();

        timer = null;
    }

    public void Dispose()
    {
        timer?.Dispose();
    }

    private void IdleDetectionTimeoutCallback( object? state )
    {
        logger.LogDebug( $"IdleChannelMonitor [{detectionMode}] triggered." );

        var channel = (Channel)state!;

        // attempt to poll channel socket if auto mode is being used
        if ( ( detectionMode == IdleDetectionMode.Auto ) && !channel.IsConnected() )
        {
            logger.LogWarning( $"Channel doesn't seem to be active anymore. Closing..." );

            Stop();

            channel.CloseAsync()
                .ConfigureAwait( false )
                .GetAwaiter()
                .GetResult();

            return;
        }

        if ( timeout == TimeSpan.Zero )
        {
            // no hard timeout
            // this is possible only with Auto mode
            return;
        }

        var lastReceived = channel.LastReceived ?? channel.Created;
        var lastSent = channel.LastSent ?? channel.Created;

        var idleReceived = DateTimeOffset.UtcNow - lastReceived;
        var idleSent = DateTimeOffset.UtcNow - lastSent;
        var idleMin = TimeSpan.FromSeconds( Math.Min( idleReceived.TotalSeconds, idleSent.TotalSeconds ) );

        var isIdle = false;

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
            var seconds = (int)timeout.TotalSeconds;
            logger.LogWarning( $"Channel has been idle for more than {seconds} seconds. Closing..." );

            Stop();

            channel.CloseAsync()
                .ConfigureAwait( false )
                .GetAwaiter()
                .GetResult();
        }
    }
}
