using Faactory.Channels.Buffers;

namespace Faactory.Channels;

/// <summary>
/// Extension methods for <see cref="DetachedPipeline"/>.
/// </summary>
public static class DetachedPipelineExtensions
{
    /// <summary>
    /// Runs the detached pipeline with the provided byte array as input.
    /// </summary>
    /// <param name="pipeline">The detached pipeline to run.</param>
    /// <param name="buffer">The byte array to process through the pipeline.</param>
    public static Task RunAsync( this DetachedPipeline pipeline, byte[] buffer )
        => pipeline.RunAsync( new ReadableByteBuffer( buffer ) );

    /// <summary>
    /// Runs the detached pipeline with the provided byte array as input and the specified endianness.
    /// </summary>
    /// <param name="pipeline">The detached pipeline to run.</param>
    /// <param name="buffer">The byte array to process through the pipeline.</param>
    /// <param name="endianness">The endianness to use for the byte buffer.</param>
    public static Task RunAsync( this DetachedPipeline pipeline, byte[] buffer, Endianness endianness )
        => pipeline.RunAsync( new ReadableByteBuffer( buffer, endianness ) );
}
