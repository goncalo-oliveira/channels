using Faactory.Channels.Adapters;
using Faactory.Channels.Handlers;

namespace Faactory.Channels;

public interface IChannelPipelineBuilder
{
    IEnumerable<IChannelService> ChannelServices { get; }
    IServiceProvider PipelineServices { get; }

    IChannelPipelineBuilder AddInputAdapter<TAdapter>() where TAdapter : class, IChannelAdapter, IInputChannelAdapter;
    IChannelPipelineBuilder AddInputAdapter( Func<IServiceProvider, IInputChannelAdapter> implementationFactory );
    IChannelPipelineBuilder AddInputAdapter( IChannelAdapter adapter );

    IChannelPipelineBuilder AddOutputAdapter<TAdapter>() where TAdapter : class, IChannelAdapter, IOutputChannelAdapter;
    IChannelPipelineBuilder AddOutputAdapter( Func<IServiceProvider, IOutputChannelAdapter> implementationFactory );
    IChannelPipelineBuilder AddOutputAdapter( IChannelAdapter adapter );

    IChannelPipelineBuilder AddInputHandler<THandler>() where THandler : class, IChannelHandler;
    IChannelPipelineBuilder AddInputHandler( Func<IServiceProvider, IChannelHandler> implementationFactory );
    IChannelPipelineBuilder AddInputHandler( IChannelHandler handler );

    IChannelPipelineBuilder AddChannelService<TService>() where TService : class, IChannelService;
    IChannelPipelineBuilder AddChannelService( Func<IServiceProvider, IChannelService> implementationFactory );
    IChannelPipelineBuilder AddChannelService( IChannelService service );

    IChannelPipeline BuildInputPipeline();
    IChannelPipeline BuildOutputPipeline();
}
