namespace Faactory.Channels.Adapters;

/// <summary>
/// An anonymous channel adapter
/// </summary>
/// <typeparam name="T">The expected data type</typeparam>
public sealed class AnonymousChannelAdapter<T> : ChannelAdapter<T>, IInputChannelAdapter, IOutputChannelAdapter
{
    private readonly Func<IAdapterContext, T, CancellationToken, Task> execute;

    /// <summary>
    /// Creates a new instance of the <see cref="AnonymousChannelAdapter{T}"/> class.
    /// </summary>
    /// <param name="func">The function to execute for the adapter</param>
    public AnonymousChannelAdapter( Func<IAdapterContext, T, CancellationToken, Task> func )
    {
        execute = func;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="AnonymousChannelAdapter{T}"/> class.
    /// </summary>
    /// <param name="func">The function to execute for the adapter</param>
    public AnonymousChannelAdapter( Func<IAdapterContext, T, Task> func )
    {
        execute = ( context, data, cancellationToken ) => func( context, data );
    }

    /// <summary>
    /// Creates a new instance of the <see cref="AnonymousChannelAdapter{T}"/> class.
    /// </summary>
    /// <param name="action">The action to execute for the adapter</param>
    public AnonymousChannelAdapter( Action<IAdapterContext, T> action )
    {
        execute = ( context, data, cancellationToken ) =>
        {
            action( context, data );

            return Task.CompletedTask;
        };
    }

    /// <summary>
    /// Executes the adapter logic.
    /// </summary>
    public override Task ExecuteAsync( IAdapterContext context, T data, CancellationToken cancellationToken )
        => execute( context, data, cancellationToken );
}
