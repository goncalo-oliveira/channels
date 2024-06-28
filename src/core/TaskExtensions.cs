namespace Faactory.Channels.Tasks;

public static class TaskExtensions
{
    /// <summary>
    /// Waits for a task to complete if it is not in a completion state
    /// </summary>
    public static void WaitForCompletion( this Task task )
    {
        if ( task is null )
        {
            return;
        }

        try
        {
            if ( !task.IsCompleted && !task.IsCanceled && !task.IsFaulted )
            {
                task.Wait();
            }
        }
        catch { }
    }

    /// <summary>
    /// Waits for a task to complete if it is not in a completion state
    /// </summary>
    public static async Task WaitForCompletionAsync( this Task task, CancellationToken cancellationToken )
    {
        if ( task is null )
        {
            return;
        }

        try
        {
            if ( !task.IsCompleted && !task.IsCanceled && !task.IsFaulted )
            {
                await task.WaitAsync( cancellationToken );
            }
        }
        catch { }
    }

    /// <summary>
    /// Tries to dispose a task if it is in a completion state
    /// </summary>
    public static void TryDispose( this Task task )
    {
        if ( task is null )
        {
            return;
        }

        /*
        A task can only be disposed if it is in a completion state (RanToCompletion, Faulted or Canceled)
        */
        try
        {
            if ( !task.IsCompleted && !task.IsCanceled && !task.IsFaulted )
            {
                return;
            }

            task.Dispose();
        }
        catch { }
    }
}
