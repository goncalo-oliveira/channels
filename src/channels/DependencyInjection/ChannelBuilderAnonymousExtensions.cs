using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Faactory.Channels.Adapters;
using Faactory.Channels.Handlers;

namespace Faactory.Channels;

public static class ChannelBuilderAnonymousExtensions
{
    /// <summary>
    /// Adds a transient service for the anonymous channel adapter to the input pipeline
    /// </summary>
    /// <param name="action">The anonymous adapter action</param>
    /// <typeparam name="TData">The data type expected by the adapter</typeparam>
    public static IChannelBuilder AddInputAdapter<TData>( this IChannelBuilder builder, Action<IAdapterContext, TData> action )
    {
        builder.Services.AddTransient<IInputChannelAdapter, AnonymousChannelAdapter<TData>>( provider =>
        {
            return new AnonymousChannelAdapter<TData>( async ( context, data ) =>
            {
                await Task.Run( () => action.Invoke( context, data ) );
            } );
        });

        return ( builder );
    }

    /// <summary>
    /// Adds a transient service for the anonymous channel adapter to the input pipeline
    /// </summary>
    /// <param name="action">The anonymous adapter action</param>
    /// <typeparam name="TData">The data type expected by the adapter</typeparam>
    public static IChannelBuilder AddInputAdapter<TData>( this IChannelBuilder builder, Action<IServiceProvider, IAdapterContext, TData> action )
    {
        builder.Services.AddTransient<IInputChannelAdapter, AnonymousChannelAdapter<TData>>( provider =>
        {
            return new AnonymousChannelAdapter<TData>( async ( context, data ) =>
            {
                await Task.Run( () => action.Invoke( provider, context, data ) );
            } );
        });

        return ( builder );
    }

    /// <summary>
    /// Adds a transient service for the anonymous channel handler to the input pipeline
    /// </summary>
    /// <param name="action">The anonymous handler action</param>
    /// <typeparam name="TData">The data type expected by the handler</typeparam>
    public static IChannelBuilder AddInputHandler<TData>( this IChannelBuilder builder, Action<IChannelContext, TData> action )
    {
        builder.Services.AddTransient<IChannelHandler, AnonymousChannelHandler<TData>>( provider =>
        {
            return new AnonymousChannelHandler<TData>( async ( context, data ) =>
            {
                await Task.Run( () => action.Invoke( context, data ) );
            } );
        });

        return ( builder );
    }

    /// <summary>
    /// Adds a transient service for the anonymous channel handler to the input pipeline
    /// </summary>
    /// <param name="action">The anonymous handler action</param>
    /// <typeparam name="TData">The data type expected by the handler</typeparam>
    public static IChannelBuilder AddInputHandler<TData>( this IChannelBuilder builder, Action<IServiceProvider, IChannelContext, TData> action )
    {
        builder.Services.AddTransient<IChannelHandler, AnonymousChannelHandler<TData>>( provider =>
        {
            return new AnonymousChannelHandler<TData>( async ( context, data ) =>
            {
                await Task.Run( () => action.Invoke( provider, context, data ) );
            } );
        });

        return ( builder );
    }

    /// <summary>
    /// Adds a transient service for the anonymous channel adapter to the output pipeline
    /// </summary>
    /// <param name="action">The anonymous adapter action</param>
    /// <typeparam name="TData">The data type expected by the adapter</typeparam>
    public static IChannelBuilder AddOutputAdapter<TData>( this IChannelBuilder builder, Action<IAdapterContext, TData> action )
    {
        builder.Services.AddTransient<IOutputChannelAdapter, AnonymousChannelAdapter<TData>>( provider =>
        {
            return new AnonymousChannelAdapter<TData>( async ( context, data ) =>
            {
                await Task.Run( () => action.Invoke( context, data ) );
            } );
        });

        return ( builder );
    }

    /// <summary>
    /// Adds a transient service for the anonymous channel adapter to the output pipeline
    /// </summary>
    /// <param name="action">The anonymous adapter action</param>
    /// <typeparam name="TData">The data type expected by the adapter</typeparam>
    public static IChannelBuilder AddOutputAdapter<TData>( this IChannelBuilder builder, Action<IServiceProvider, IAdapterContext, TData> action )
    {
        builder.Services.AddTransient<IOutputChannelAdapter, AnonymousChannelAdapter<TData>>( provider =>
        {
            return new AnonymousChannelAdapter<TData>( async ( context, data ) =>
            {
                await Task.Run( () => action.Invoke( provider, context, data ) );
            } );
        });

        return ( builder );
    }
}
