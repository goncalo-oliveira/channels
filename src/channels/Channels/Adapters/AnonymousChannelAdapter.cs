using Microsoft.Extensions.Logging;

namespace Faactory.Channels.Adapters;

/// <summary>
/// An anonymous channel adapter
/// </summary>
/// <typeparam name="T">The expected data type</typeparam>
public sealed class AnonymousChannelAdapter<T> : ChannelAdapter<T>
{
    private readonly Func<IAdapterContext, T, Task> execute;

    public AnonymousChannelAdapter( Func<IAdapterContext, T, Task> func )
    {
        execute = func;
    }

    public override Task ExecuteAsync( IAdapterContext context, T data )
        => execute( context, data );
}
