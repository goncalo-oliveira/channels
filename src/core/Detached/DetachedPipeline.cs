using Faactory.Channels.Adapters;
using Faactory.Channels.Buffers;

namespace Faactory.Channels;

/// <summary>
/// A detached pipeline to use outside a Channels context
/// </summary>
public sealed class DetachedPipeline
{
    public List<IChannelAdapter> Adapters { get; } = new List<IChannelAdapter>();
    public DetachedContext Context { get; } = new DetachedContext();

    public DetachedPipeline AddAdapter( IChannelAdapter adapter )
    {
        Adapters.Add( adapter );

        return ( this );
    }

    public DetachedPipeline AddChannelService( IChannelService service )
    {
        ( (DetachedChannel)Context.Channel ).AddChannelService( service );

        return ( this );
    }

    public async Task RunAsync( IByteBuffer buffer )
    {
        var data = new object[]
        {
            buffer
        };

        foreach ( var adapter in Adapters )
        {
            var forwarded = new List<object>();

            foreach ( var item in data )
            {
                Context.Clear();

                await adapter.ExecuteAsync( Context, item );

                forwarded.AddRange( Context.Forwarded );
            }

            data = forwarded.ToArray();
        }

        Context.Clear();
        Context.ForwardMany( data );
    }
}
