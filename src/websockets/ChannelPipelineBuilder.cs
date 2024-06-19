using Faactory.Channels.Adapters;
using Faactory.Channels.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Faactory.Channels;

internal sealed class ChannelPipelineBuilder : IChannelPipelineBuilder
{
    private readonly List<IChannelAdapter> inputAdapters = [];
    private readonly List<IChannelAdapter> outputAdapters = [];
    private readonly List<IChannelHandler> inputHandlers = [];
    private readonly List<IChannelHandler> outputHandlers = [];
    private readonly List<IChannelService> channelServices = [];

    public ChannelPipelineBuilder( IServiceCollection services )
    {
        PipelineServices = services.BuildServiceProvider();
    }

    public ChannelPipelineBuilder( IServiceProvider serviceProvider )
    {
        PipelineServices = serviceProvider;
    }

    public IEnumerable<IChannelService> ChannelServices => channelServices.ToArray();

    public IServiceProvider PipelineServices { get; }

    public IChannelPipelineBuilder AddInputAdapter<TAdapter>() where TAdapter : class, IChannelAdapter, IInputChannelAdapter
    {
        var adapter = PipelineServices.GetAdapters<IInputChannelAdapter>()
            .SingleOrDefault( a => a.GetType() == typeof( TAdapter ) );

        if ( adapter is null )
        {
            throw new InvalidOperationException( $"The input adapter {typeof( TAdapter ).Name} is not registered." );
        }

        inputAdapters.Add( adapter );

        return this;
    }

    public IChannelPipelineBuilder AddInputAdapter( Func<IServiceProvider, IInputChannelAdapter> implementationFactory )
    {
        var adapter = implementationFactory( PipelineServices );

        if ( adapter is not IChannelAdapter )
        {
            throw new InvalidOperationException( "The implementation factory must return an IChannelAdapter instance." );
        }

        inputAdapters.Add( (IChannelAdapter)adapter );

        return this;
    }

    public IChannelPipelineBuilder AddInputAdapter( IChannelAdapter adapter )
    {
        if ( adapter is not IInputChannelAdapter )
        {
            throw new InvalidOperationException( "The adapter must implement IInputChannelAdapter." );
        }

        inputAdapters.Add( adapter );

        return this;
    }

    // internal IChannelPipelineBuilder AddRegisteredInputAdapters()
    // {
    //     inputAdapters.AddRange(
    //         PipelineServices.GetAdapters<IInputChannelAdapter>()
    //     );

    //     return this;
    // }

    //

    public IChannelPipelineBuilder AddOutputAdapter<TAdapter>() where TAdapter : class, IChannelAdapter, IOutputChannelAdapter
    {
        var adapter = PipelineServices.GetAdapters<IOutputChannelAdapter>()
            .SingleOrDefault( a => a.GetType() == typeof( TAdapter ) );

        if ( adapter is null )
        {
            throw new InvalidOperationException( $"The output adapter {typeof( TAdapter ).Name} is not registered." );
        }

        outputAdapters.Add( adapter );

        return this;
    }

    public IChannelPipelineBuilder AddOutputAdapter( Func<IServiceProvider, IOutputChannelAdapter> implementationFactory )
    {
        var adapter = implementationFactory( PipelineServices );

        if ( adapter is not IChannelAdapter )
        {
            throw new InvalidOperationException( "The implementation factory must return an IChannelAdapter instance." );
        }

        outputAdapters.Add( (IChannelAdapter)adapter );

        return this;
    }

    public IChannelPipelineBuilder AddOutputAdapter( IChannelAdapter adapter )
    {
        if ( adapter is not IOutputChannelAdapter )
        {
            throw new InvalidOperationException( "The adapter must implement IOutputChannelAdapter." );
        }

        outputAdapters.Add( adapter );

        return this;
    }

    internal IChannelPipelineBuilder AddRegisteredOutputAdapters()
    {
        outputAdapters.AddRange(
            PipelineServices.GetAdapters<IOutputChannelAdapter>()
        );

        return this;
    }

    //

    public IChannelPipelineBuilder AddInputHandler<THandler>() where THandler : class, IChannelHandler
    {
        var handler = PipelineServices.GetHandlers()
            .SingleOrDefault( a => a.GetType() == typeof( THandler ) );

        if ( handler is null )
        {
            throw new InvalidOperationException( $"The input handler {typeof( THandler ).Name} is not registered." );
        }

        inputHandlers.Add( handler );

        return this;
    }

    public IChannelPipelineBuilder AddInputHandler( Func<IServiceProvider, IChannelHandler> implementationFactory )
    {
        inputHandlers.Add(
            implementationFactory( PipelineServices )
        );

        return this;
    }

    public IChannelPipelineBuilder AddInputHandler( IChannelHandler handler )
    {
        inputHandlers.Add( handler );

        return this;
    }

    // internal IChannelPipelineBuilder AddRegisteredInputHandlers()
    // {
    //     inputHandlers.AddRange(
    //         PipelineServices.GetHandlers()
    //     );

    //     return this;
    // }

    //

    public IChannelPipelineBuilder AddOutputHandler( Func<IServiceProvider, IChannelHandler> implementationFactory )
    {
        outputHandlers.Add(
            implementationFactory( PipelineServices )
        );

        return this;
    }

    public IChannelPipelineBuilder AddOutputHandler( IChannelHandler handler )
    {
        outputHandlers.Add( handler );

        return this;
    }

    //

    public IChannelPipelineBuilder AddChannelService<TService>() where TService : class, IChannelService
    {
        var service = PipelineServices.GetServices<IChannelService>()
            .SingleOrDefault( s => s.GetType() == typeof( TService ) );

        if ( service is null )
        {
            throw new InvalidOperationException( $"The channel service {typeof( TService ).Name} is not registered." );
        }

        channelServices.Add( service );

        return this;
    }

    public IChannelPipelineBuilder AddChannelService( Func<IServiceProvider, IChannelService> implementationFactory )
    {
        channelServices.Add(
            implementationFactory( PipelineServices )
        );

        return this;
    }

    public IChannelPipelineBuilder AddChannelService( IChannelService service )
    {
        channelServices.Add( service );

        return this;
    }

    // internal IChannelPipelineBuilder AddRegisteredChannelServices()
    // {
    //     channelServices.AddRange(
    //         PipelineServices.GetServices<IChannelService>()
    //     );

    //     return this;
    // }

    //

    public IChannelPipeline BuildInputPipeline()
    {
        return new ChannelPipeline(
            PipelineServices.GetRequiredService<ILoggerFactory>(),
            inputAdapters,
            inputHandlers
        );
    }

    public IChannelPipeline BuildOutputPipeline()
    {
        return new ChannelPipeline(
            PipelineServices.GetRequiredService<ILoggerFactory>(),
            outputAdapters,
            outputHandlers
        );
    }

    //

    /// <summary>
    /// Creates a default channel pipeline builder with all registered adapters, handlers and services.
    /// </summary>
    /// <param name="provider">The service provider</param>
    /// <param name="registerServices">Whether to add registered channel services. Default is true.</param>
    public static IChannelPipelineBuilder CreateDefault( IServiceProvider provider, bool registerServices = true )
    {
        var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
        var pipelineBuilder = new ChannelPipelineBuilder( provider );

        // add all registered input adapters
        pipelineBuilder.AddRegisteredInputAdapters();

        // add all registered input handlers
        pipelineBuilder.AddRegisteredInputHandlers();

        // add all registered output adapters
        pipelineBuilder.AddRegisteredOutputAdapters();

        // add the default output handler
        pipelineBuilder.AddOutputHandler( new OutputChannelHandler( loggerFactory ) );

        // register all channel services
        if ( registerServices )
        {
            pipelineBuilder.AddRegisteredChannelServices();
        }

        return pipelineBuilder;
    }
}
