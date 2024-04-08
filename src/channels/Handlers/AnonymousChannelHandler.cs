namespace Faactory.Channels.Handlers;

/// <summary>
/// An anonymous channel handler
/// </summary>
/// <typeparam name="T">The expected data type</typeparam>
public sealed class AnonymousChannelHandler<T>( Func<IChannelContext, T, Task> func ) : ChannelHandler<T>
{
    private readonly Func<IChannelContext, T, Task> execute = func;

    public override Task ExecuteAsync( IChannelContext context, T data )
        => execute( context, data );
}
