namespace Faactory.Channels.Handlers;

/// <summary>
/// An anonymous channel handler
/// </summary>
/// <typeparam name="T">The expected data type</typeparam>
public sealed class AnonymousChannelHandler<T> : ChannelHandler<T>
{
    private readonly Func<IChannelContext, T, CancellationToken, Task> execute;

    public AnonymousChannelHandler( Func<IChannelContext, T, CancellationToken, Task> func )
    {
        execute = func;
    }

    public AnonymousChannelHandler( Func<IChannelContext, T, Task> func )
    {
        execute = ( context, data, cancellationToken ) => func( context, data );
    }

    public AnonymousChannelHandler( Action<IChannelContext, T> action )
    {
        execute = ( context, data, cancellationToken ) =>
        {
            action( context, data );

            return Task.CompletedTask;
        };
    }

    public override Task ExecuteAsync( IChannelContext context, T data, CancellationToken cancellationToken )
        => execute( context, data, cancellationToken );
}
