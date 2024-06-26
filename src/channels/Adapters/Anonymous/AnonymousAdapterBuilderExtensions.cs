using Faactory.Channels.Adapters;
using Faactory.Channels.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

public static class AnonymousAdapterChannelBuilderExtensions
{
    /// <summary>
    /// Adds a transient service for the anonymous channel adapter to the input pipeline
    /// </summary>
    /// <param name="action">The anonymous adapter action</param>
    /// <typeparam name="TData">The data type expected by the adapter</typeparam>
    public static IChannelBuilder AddInputAdapter<TData>( this IChannelBuilder builder, Action<IAdapterContext, TData> action )
    {
        builder.Services.AddKeyedTransient<IInputChannelAdapter, AnonymousChannelAdapter<TData>>( builder.Name, ( provider, _ ) =>
        {
            return new AnonymousChannelAdapter<TData>( async ( context, data ) =>
            {
                await Task.Run( () => action.Invoke( context, data ) )
                    .ConfigureAwait( false );
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
        builder.Services.AddKeyedTransient<IInputChannelAdapter, AnonymousChannelAdapter<TData>>( builder.Name, ( provider, _ ) =>
        {
            return new AnonymousChannelAdapter<TData>( async ( context, data ) =>
            {
                await Task.Run( () => action.Invoke( provider, context, data ) )
                    .ConfigureAwait( false );
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
        builder.Services.AddKeyedTransient<IOutputChannelAdapter, AnonymousChannelAdapter<TData>>( builder.Name, ( provider, _ ) =>
        {
            return new AnonymousChannelAdapter<TData>( async ( context, data ) =>
            {
                await Task.Run( () => action.Invoke( context, data ) )
                    .ConfigureAwait( false );
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
        builder.Services.AddKeyedTransient<IOutputChannelAdapter, AnonymousChannelAdapter<TData>>( builder.Name, ( provider, _ ) =>
        {
            return new AnonymousChannelAdapter<TData>( async ( context, data ) =>
            {
                await Task.Run( () => action.Invoke( provider, context, data ) )
                    .ConfigureAwait( false );
            } );
        });

        return ( builder );
    }
}
