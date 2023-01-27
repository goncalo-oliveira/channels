namespace Faactory.Channels;

public abstract class ChannelService : IChannelService
{
    private Task? task;
    private CancellationTokenSource? cancellationTokenSource;

    public IChannel? Channel { get; private set; }

    public virtual Task StartAsync( IChannel channel, CancellationToken cancellationToken )
    {
        Channel = channel;

        cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource( cancellationToken );

        task = ExecuteAsync( cancellationTokenSource.Token );

        return task.IsCompleted
            ? task
            : Task.CompletedTask;
    }

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

            using ( CancellationTokenRegistration tokenRegistration = cancellationToken.Register( x => ((TaskCompletionSource<object>?)x)?.SetCanceled(), taskCompletion ) )
            {
                await Task.WhenAny( task, taskCompletion.Task )
                    .ConfigureAwait( false );
            }
        }
    }

    public virtual void Dispose()
    {
        cancellationTokenSource?.Cancel();
        Channel = null;
    }

    protected abstract Task ExecuteAsync( CancellationToken cancellationToken );
}
