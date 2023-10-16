namespace Faactory.Channels;

/// <summary>
/// Base class for implementing a long-running service with the same lifespan of a channel.
/// </summary>
public abstract class ChannelService : IChannelService
{
    private Task? task;
    private CancellationTokenSource? cancellationTokenSource;

    /// <summary>
    /// Gets the channel that owns the service.
    /// </summary>
    /// <returns>The channel instance that owns the service; null if the service hasn't started yet.</returns>
    protected IChannel Channel { get; private set; } = NullChannel.Instance;

    /// <summary>
    /// Triggered when the channel is created and ready to start the service.
    /// </summary>
    public virtual Task StartAsync( IChannel channel, CancellationToken cancellationToken )
    {
        Channel = channel;

        cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource( cancellationToken );

        task = ExecuteAsync( cancellationTokenSource.Token );

        return task.IsCompleted
            ? task
            : Task.CompletedTask;
    }

    /// <summary>
    /// Triggered when the channel is performing a graceful shutdown.
    /// </summary>
    public virtual async Task StopAsync( CancellationToken cancellationToken )
    {
        if ( task is null )
        {
            return;
        }

        try
        {
            cancellationTokenSource?.Cancel();
        }
        finally
        {
            var taskCompletion = new TaskCompletionSource<object>();

            using CancellationTokenRegistration tokenRegistration = cancellationToken.Register(
                x => ( (TaskCompletionSource<object>?)x )?.SetCanceled(),
                taskCompletion
            );
            await Task.WhenAny( task, taskCompletion.Task )
                .ConfigureAwait( false );
        }
    }

    public virtual void Dispose()
    {
        GC.SuppressFinalize( this );

        cancellationTokenSource?.Cancel();
        Channel = NullChannel.Instance;
    }

    /// <summary>
    /// Called when the service starts. Should return a task that represents the lifetime of the long-running operation.
    /// </summary>
    protected abstract Task ExecuteAsync( CancellationToken cancellationToken );
}
