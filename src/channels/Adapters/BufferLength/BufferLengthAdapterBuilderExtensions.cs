using Faactory.Channels.Adapters;
using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

public static class BufferLengthAdapterChannelBuilderExtensions
{
    /// <summary>
    /// Adds a buffer length adapter to the input pipeline
    /// </summary>
    public static IChannelBuilder AddBufferLengthInputAdapter<TChannelBuilder>( this IChannelBuilder builder, Action<BufferLengthAdapterOptions>? configure = null )
    {
        builder.AddInputAdapter<BufferLengthAdapter>();

        if ( configure != null )
        {
            builder.Services.Configure( configure );
        }

        return builder;
    }
}
