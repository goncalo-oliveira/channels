namespace Faactory.Channels.Handlers;

/// <summary>
/// An anonymous channel handler
/// </summary>
/// <typeparam name="T">The expected data type</typeparam>
public sealed class AnonymousChannelHandler<T> : ChannelHandler<T>
{
    private readonly Func<IChannelContext, T, CancellationToken, Task> execute;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnonymousChannelHandler{T}"/> class.
    /// </summary>
    /// <param name="func">The function to execute when the channel handler is invoked</param>
    public AnonymousChannelHandler( Func<IChannelContext, T, CancellationToken, Task> func )
    {
        execute = func;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AnonymousChannelHandler{T}"/> class.
    /// </summary>
    /// <param name="func">The function to execute when the channel handler is invoked</param>
    public AnonymousChannelHandler( Func<IChannelContext, T, Task> func )
    {
        execute = ( context, data, cancellationToken ) => func( context, data );
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AnonymousChannelHandler{T}"/> class.
    /// </summary>
    /// <param name="action">The action to execute when the channel handler is invoked</param>
    public AnonymousChannelHandler( Action<IChannelContext, T> action )
    {
        execute = ( context, data, cancellationToken ) =>
        {
            action( context, data );

            return Task.CompletedTask;
        };
    }

    /// <summary>
    /// Executes the channel handler logic
    /// </summary>
    public override Task ExecuteAsync( IChannelContext context, T data, CancellationToken cancellationToken )
        => execute( context, data, cancellationToken );
}
