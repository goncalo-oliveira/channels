using Faactory.Channels.Buffers;

namespace Faactory.Channels;

public static class DetachedPipelineExtensions
{
    public static Task RunAsync( this DetachedPipeline pipeline, byte[] buffer )
        => pipeline.RunAsync( new WrappedByteBuffer( buffer ) );

    public static Task RunAsync( this DetachedPipeline pipeline, byte[] buffer, Endianness endianness )
        => pipeline.RunAsync( new WrappedByteBuffer( buffer, endianness ) );
}
