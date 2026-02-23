using Faactory.Channels.Adapters;
using Faactory.Channels.Buffers;

namespace Faactory.Channels;

/// <summary>
/// A detached pipeline to use outside a Channels context
/// </summary>
public sealed class DetachedPipeline
{
    /// <summary>
    /// A list of adapters to execute in the pipeline
    /// </summary>
    public List<IChannelAdapter> Adapters { get; } = [];

    /// <summary>
    /// The context to use for the pipeline execution
    /// </summary>
    public DetachedContext Context { get; } = new DetachedContext();

    /// <summary>
    /// Adds an adapter to the pipeline
    /// </summary>
    public DetachedPipeline AddAdapter( IChannelAdapter adapter )
    {
        Adapters.Add( adapter );

        return this;
    }

    /// <summary>
    /// Adds a channel service to the pipeline context
    /// </summary>
    public DetachedPipeline AddChannelService( IChannelService service )
    {
        ( (DetachedChannel)Context.Channel ).AddChannelService( service );

        return this;
    }

    /// <summary>
    /// Runs the pipeline with the given object as input
    /// </summary>
    /// <param name="obj">The input object to process through the pipeline.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task RunAsync( object obj, CancellationToken cancellationToken = default )
    {
        var data = new object[]
        {
            obj
        };

        // If the input object is a byte buffer, set the context's buffer endianness to match it
        if ( typeof( IByteBuffer ).IsAssignableFrom( obj.GetType() ) )
        {
            Context.BufferEndianness = ( (IByteBuffer)obj ).Endianness;
        }

        foreach ( var adapter in Adapters )
        {
            var forwarded = new List<object>();

            foreach ( var item in data )
            {
                Context.Clear();

                await adapter.ExecuteAsync( Context, item, cancellationToken );

                forwarded.AddRange( Context.Forwarded );
            }

            data = forwarded.ToArray();
        }

        Context.Clear();
        Context.ForwardMany( data );
    }
}
