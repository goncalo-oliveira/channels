using Faactory.Channels.Adapters;

namespace Faactory.Channels;

public static class BufferLengthAdapterChannelBuilderExtensions
{
    /// <summary>
    /// Adds a buffer length adapter to the input pipeline
    /// </summary>
    public static IChannelBuilder AddBufferLengthInputAdapter<TChannelBuilder>( this IChannelBuilder<TChannelBuilder> builder, Action<BufferLengthAdapterOptions>? configure = null ) where TChannelBuilder : IChannelBuilder<TChannelBuilder>
    {
        builder.AddInputAdapter<BufferLengthAdapter>();

        if ( configure != null )
        {
            builder.Configure( configure );
        }

        return builder;
    }
}
