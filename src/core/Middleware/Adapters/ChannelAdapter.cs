using Microsoft.Extensions.Logging;

namespace Faactory.Channels.Adapters;

/// <summary>
/// Base class for implementing a channel adapter
/// </summary>
/// <typeparam name="T">The data type</typeparam>
public abstract class ChannelAdapter<T> : ChannelMiddleware<T>, IChannelAdapter
{
    public ChannelAdapter()
    {}

    /// <summary>
    /// Called when matching data (T) is received by the adapter
    /// </summary>
    public abstract Task ExecuteAsync( IAdapterContext context, T data, CancellationToken cancellationToken );

    public override Task ExecuteAsync( IChannelContext context, T data, CancellationToken cancellationToken )
        => ExecuteAsync( (IAdapterContext)context, data, cancellationToken );

    public Task ExecuteAsync( IAdapterContext context, object data, CancellationToken cancellationToken )
        => base.ExecuteAsync( context, data, cancellationToken );

    protected override void OnDataNotSuitable( IChannelContext context, object data )
        => ( (IAdapterContext)context ).Forward( data );
}
