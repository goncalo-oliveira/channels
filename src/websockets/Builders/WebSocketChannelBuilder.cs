using Faactory.Channels.Adapters;
using Faactory.Channels.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels.WebSockets;

/*
if we used a keyed service, we could have different versions of the pipeline;
we register middleware services with a key and the same with channel services
then the factory will use the key to resolve the correct services
*/

internal sealed class WebSocketChannelBuilder( IServiceCollection services ) : ChannelBuilder<IWebSocketChannelBuilder>( services ), IWebSocketChannelBuilder
{
    public IWebSocketChannelBuilder Configure( Action<ChannelOptions> configure )
    {
        Services.Configure( configure );

        return Self();
    }

    protected override IWebSocketChannelBuilder Self() => this;
}

// internal sealed class WebSocketChannelBuilder( string name, IServiceCollection services ) : IWebSocketChannelBuilder
// {
//     private readonly string name = name;

//     public IServiceCollection Services { get; } = services;

//     public IWebSocketChannelBuilder Configure( Action<ChannelOptions> configure )
//     {
//         Services.Configure( configure );

//         return Self();
//     }

//     public IWebSocketChannelBuilder AddInputAdapter<TAdapter>() where TAdapter : class, IChannelAdapter, IInputChannelAdapter
//     {
//         Services.AddKeyedTransient<IInputChannelAdapter, TAdapter>( name );

//         return Self();
//     }

//     public IWebSocketChannelBuilder AddInputAdapter( Func<IServiceProvider, IInputChannelAdapter> implementationFactory )
//     {
//         Services.AddKeyedTransient( name, ( sp, key ) =>
//         {
//             return implementationFactory( sp );
//         } );

//         return Self();
//     }

//     public IWebSocketChannelBuilder AddOutputAdapter<TAdapter>() where TAdapter : class, IChannelAdapter, IOutputChannelAdapter
//     {
//         Services.AddKeyedTransient<IOutputChannelAdapter, TAdapter>( name );

//         return Self();
//     }

//     public IWebSocketChannelBuilder AddOutputAdapter( Func<IServiceProvider, IOutputChannelAdapter> implementationFactory )
//     {
//         Services.AddKeyedTransient( name, ( sp, key ) =>
//         {
//             return implementationFactory( sp );
//         } );

//         return Self();
//     }

//     public IWebSocketChannelBuilder AddInputHandler<THandler>() where THandler : class, IChannelHandler
//     {
//         Services.AddKeyedTransient<IChannelHandler, THandler>( name );

//         return Self();
//     }

//     public IWebSocketChannelBuilder AddInputHandler( Func<IServiceProvider, IChannelHandler> implementationFactory )
//     {
//         Services.AddKeyedTransient( name, ( sp, key ) =>
//         {
//             return implementationFactory( sp );
//         } );

//         return Self();
//     }

//     public IWebSocketChannelBuilder AddChannelService<TService>() where TService : class, IChannelService
//     {
//         Services.AddKeyedScoped<IChannelService, TService>( name );

//         return Self();
//     }

//     public IWebSocketChannelBuilder AddChannelService( Func<IServiceProvider, IChannelService> implementationFactory )
//     {
//         Services.AddKeyedScoped( name, ( sp, key ) =>
//         {
//             return implementationFactory( sp );
//         } );

//         return Self();
//     }

//     private WebSocketChannelBuilder Self() => this;
// }
