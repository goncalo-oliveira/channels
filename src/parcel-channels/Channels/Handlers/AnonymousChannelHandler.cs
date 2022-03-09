using Microsoft.Extensions.Logging;

namespace Parcel.Channels.Handlers;

/// <summary>
/// An anonymous channel handler
/// </summary>
/// <typeparam name="T">The expected data type</typeparam>
public sealed class AnonymousChannelHandler<T> : ChannelHandler<T>
{
    private readonly Func<IChannelContext, T, Task> execute;

    public AnonymousChannelHandler( Func<IChannelContext, T, Task> func )
    {
        execute = func;
    }

    public override Task ExecuteAsync( IChannelContext context, T data )
        => execute( context, data );
}
