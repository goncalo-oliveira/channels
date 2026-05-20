
namespace Faactory.Channels.Correlation;

/// <summary>
/// Awaiter for a channel response, allowing to wait for a response with cancellation support.
/// </summary>
/// <typeparam name="T">The type of the message to await.</typeparam>
internal sealed class ChannelResponseAwaiter<T> : IChannelResponseAwaiter<T>
{
    private readonly TaskCompletionSource<T> tcs = new( TaskCreationOptions.RunContinuationsAsynchronously );
    private int completed; // 0 = pending, 1 = completed

    public Task<T> WaitAsync( CancellationToken cancellationToken )
    {
        if ( Volatile.Read( ref completed ) != 0 )
        {
            return tcs.Task;
        }

        if ( cancellationToken.IsCancellationRequested )
        {
            return Task.FromCanceled<T>( cancellationToken );
        }

        if ( !cancellationToken.CanBeCanceled )
        {
            return tcs.Task;
        }

        var registration = cancellationToken.Register(
            static state =>
            {
                var (self, token) = ( (ChannelResponseAwaiter<T>, CancellationToken) )state!;

                self.TryCancel( token );
            },
            ( this, cancellationToken )
        );

        _ = tcs.Task.ContinueWith(
            static ( _, state ) => ( (CancellationTokenRegistration)state! ).Dispose(),
            registration,
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default
        );

        return tcs.Task;
    }

    /// <summary>
    /// Attempts to set the result of the awaiter. Returns false if the awaiter has already been completed or canceled.
    /// </summary>
    /// <param name="result">The result to set for the awaiter.</param>
    /// <returns>True if the result was successfully set; otherwise, false.</returns>
    public bool TrySetResult( T result )
    {
        if ( Interlocked.Exchange( ref completed, 1 ) != 0 )
        {
            return false;
        }

        return tcs.TrySetResult( result );
    }

    /// <summary>
    /// Attempts to cancel the awaiter. Returns false if the awaiter has already been completed or canceled.
    /// </summary>
    /// <returns>True if the awaiter was successfully canceled; otherwise, false.</returns>
    public bool TryCancel( CancellationToken cancellationToken )
    {
        if ( Interlocked.Exchange( ref completed, 1 ) != 0 )
        {
            return false;
        }

        return tcs.TrySetCanceled( cancellationToken );
    }

    /// <summary>
    /// Gets a value indicating whether the awaiter has been completed or canceled.
    /// </summary>
    public bool IsCompleted => Volatile.Read( ref completed ) != 0;
}
