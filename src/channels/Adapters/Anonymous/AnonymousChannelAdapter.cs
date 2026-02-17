namespace Faactory.Channels.Adapters;

/// <summary>
/// An anonymous channel adapter
/// </summary>
/// <typeparam name="T">The expected data type</typeparam>
public sealed class AnonymousChannelAdapter<T> : ChannelAdapter<T>, IInputChannelAdapter, IOutputChannelAdapter
{
    private readonly Func<IAdapterContext, T, CancellationToken, Task> execute;

    public AnonymousChannelAdapter( Func<IAdapterContext, T, CancellationToken, Task> func )
    {
        execute = func;
    }

    public AnonymousChannelAdapter( Func<IAdapterContext, T, Task> func )
    {
        execute = ( context, data, cancellationToken ) => func( context, data );
    }

    public AnonymousChannelAdapter( Action<IAdapterContext, T> action )
    {
        execute = ( context, data, cancellationToken ) =>
        {
            action( context, data );

            return Task.CompletedTask;
        };
    }

    public override Task ExecuteAsync( IAdapterContext context, T data, CancellationToken cancellationToken )
        => execute( context, data, cancellationToken );
}
