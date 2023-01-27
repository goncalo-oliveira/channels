using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Faactory.Channels;

internal sealed class IdleChannelService : IChannelService
{
    private readonly ILogger logger;
    private Timer? timer;
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

        if ( ( timeout == TimeSpan.Zero ) && ( detectionMode != IdleDetectionMode.Auto ) )
        {
            // no hard timeout is only allowed with Auto mode
            throw new ArgumentOutOfRangeException( nameof( timeout ) );
        }
    }

    public Task StartAsync( IChannel channel, CancellationToken cancellationToken )
    {
        logger.LogDebug( "Starting..." );

        if ( detectionMode == IdleDetectionMode.None )
        {
            // not enabled
            logger.LogInformation( "Canceled. Idle detection mode is set to 'None'." );

            return Task.CompletedTask;
        }

        var dueTime = ( detectionMode == IdleDetectionMode.Auto )
            ? TimeSpan.FromSeconds( 5 )
            : timeout;

        var intervalTime = ( detectionMode == IdleDetectionMode.Auto )
            ? dueTime
            : ( timeout / 2 );

        timer = new Timer( IdleDetectionTimeoutCallback
            , channel
            , dueTime, intervalTime );

        logger.LogInformation( "Started." );

        return Task.CompletedTask;
    }

    public Task StopAsync( CancellationToken cancellationToken )
    {
        logger.LogDebug( "Stopping..." );

        if ( timer == null )
        {
            // not active
            logger.LogDebug( "Already stopped." );

            return Task.CompletedTask;
        }

        try
        {
            timer?.Change( Timeout.Infinite, Timeout.Infinite );
            timer?.Dispose();
        }
        catch ( ObjectDisposedException )
        { }
        finally
        {
            timer = null;
        }

        logger.LogInformation( "Stopped." );

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        timer?.Dispose();
        timer = null;
    }

    private void IdleDetectionTimeoutCallback( object? state )
    {
        var channel = (Channel)state!;

        // attempt to poll channel socket if auto mode is being used
        if ( ( detectionMode == IdleDetectionMode.Auto ) && !channel.IsConnected() )
        {
            logger.LogWarning( $"Channel doesn't seem to be active anymore. Closing..." );

            StopAsync( CancellationToken.None )
                .ConfigureAwait( false )
                .GetAwaiter()
                .GetResult();

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

            StopAsync( CancellationToken.None )
                .ConfigureAwait( false )
                .GetAwaiter()
                .GetResult();

            channel.CloseAsync()
                .ConfigureAwait( false )
                .GetAwaiter()
                .GetResult();
        }
    }
}
