using Microsoft.Extensions.Logging;
using Faactory.Channels.Buffers;
using System.Collections;

namespace Faactory.Channels.Adapters;

/// <summary>
/// Base class for a channel adapter
/// </summary>
/// <typeparam name="T">The data type</typeparam>
public abstract class ChannelAdapter<T> : ChannelMiddleware<T>, IChannelAdapter
{
    public ChannelAdapter()
    {}

    public ChannelAdapter( ILoggerFactory loggerFactory )
    : base( loggerFactory )
    { }

    public abstract Task ExecuteAsync( IAdapterContext context, T data );

    public override Task ExecuteAsync( IChannelContext context, T data )
        => ExecuteAsync( (IAdapterContext)context, data );

    public Task ExecuteAsync( IAdapterContext context, object data )
        => base.ExecuteAsync( context, data );

    protected override void OnDataNotSuitable( IChannelContext context, object data )
    {
        Logger?.LogDebug( $"Data type '{data.GetType().Name}' is not suitable for this adapter." );

        ( (IAdapterContext)context ).Forward( data );
    }
}
