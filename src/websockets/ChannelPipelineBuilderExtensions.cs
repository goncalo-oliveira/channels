using Faactory.Channels.Adapters;
using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

public static class ChannelPipelineBuilderExtensions
{
    /// <summary>
    /// Adds all registered input adapters to the pipeline builder.
    /// </summary>
    public static IChannelPipelineBuilder AddRegisteredInputAdapters( this IChannelPipelineBuilder pipelineBuilder )
    {
        foreach ( var adapter in pipelineBuilder.PipelineServices.GetAdapters<IInputChannelAdapter>() )
        {
            pipelineBuilder.AddInputAdapter( adapter );
        }

        return pipelineBuilder;
    }

    /// <summary>
    /// Adds all registered input handlers to the pipeline builder.
    /// </summary>
    public static IChannelPipelineBuilder AddRegisteredInputHandlers( this IChannelPipelineBuilder pipelineBuilder )
    {
        foreach ( var handler in pipelineBuilder.PipelineServices.GetHandlers() )
        {
            pipelineBuilder.AddInputHandler( handler );
        }

        return pipelineBuilder;
    }

    /// <summary>
    /// Adds all registered channel services to the pipeline builder.
    /// </summary>
    public static IChannelPipelineBuilder AddRegisteredChannelServices( this IChannelPipelineBuilder pipelineBuilder )
    {
        foreach ( var service in pipelineBuilder.PipelineServices.GetServices<IChannelService>() )
        {
            pipelineBuilder.AddChannelService( service );
        }

        return pipelineBuilder;
    }
}
