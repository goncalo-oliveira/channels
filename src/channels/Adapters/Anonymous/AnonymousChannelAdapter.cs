namespace Faactory.Channels.Adapters;

/// <summary>
/// An anonymous channel adapter
/// </summary>
/// <typeparam name="T">The expected data type</typeparam>
public sealed class AnonymousChannelAdapter<T>( Func<IAdapterContext, T, Task> func ) : ChannelAdapter<T>, IInputChannelAdapter, IOutputChannelAdapter
{
    private readonly Func<IAdapterContext, T, Task> execute = func;

    public override Task ExecuteAsync( IAdapterContext context, T data )
        => execute( context, data );
}
