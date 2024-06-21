// using Faactory.Channels.Adapters;
// using Faactory.Channels.Handlers;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Logging;

// namespace Faactory.Channels;

// /// <summary>
// /// Channel pipeline factory
// /// </summary>
// internal sealed class ChannelPipelineFactory( IServiceProvider serviceProvider )
// {
//     private readonly IServiceProvider provider = serviceProvider;

//     public IEnumerable<IChannelAdapter> GetInputAdapters()
//         => provider.GetAdapters<IInputChannelAdapter>( "__default" );

//     public IEnumerable<IChannelHandler> GetInputHandlers()
//         => provider.GetHandlers( "__default" );

//     public IEnumerable<IChannelAdapter> GetOutputAdapters()
//         => provider.GetAdapters<IOutputChannelAdapter>( "__default" );

//     public IChannelPipeline CreateInputPipeline()
//     {
//         var adapters = GetInputAdapters();
//         var handlers = GetInputHandlers();

//         return CreatePipeline( adapters, handlers );
//     }

//     public IChannelPipeline CreateOutputPipeline()
//     {
//         var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
//         var adapters = GetOutputAdapters();
//         var handlers = new IChannelHandler[]
//         {
//             new OutputChannelHandler( loggerFactory )
//         };

//         return new ChannelPipeline( loggerFactory, adapters, handlers );
//     }

//     public IChannelPipeline CreatePipeline( IEnumerable<IChannelAdapter> adapters, IEnumerable<IChannelHandler>? handlers = null )
//     {
//         var loggerFactory = provider.GetService<ILoggerFactory>()
//             ?? Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance;

//         return new ChannelPipeline( loggerFactory, adapters, handlers );
//     }

//     public static IChannelPipeline CreatePipeline( ILoggerFactory loggerFactory, IChannelAdapter[] adapters, IChannelHandler[]? handlers = null )
//         => new ChannelPipeline( loggerFactory, adapters, handlers );
// }
