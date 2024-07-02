using Faactory.Channels.Adapters;

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
        builder.AddInputAdapter( ( provider ) =>
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
        builder.AddInputAdapter( ( provider ) =>
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
        builder.AddOutputAdapter( ( provider ) =>
        {
            return new AnonymousChannelAdapter<TData>( async ( context, data ) =>
            {
                await Task.Run( () => action.Invoke( context, data ) )
                    .ConfigureAwait( false );
            } );
        });

        return builder;
    }

    /// <summary>
    /// Adds a transient service for the anonymous channel adapter to the output pipeline
    /// </summary>
    /// <param name="action">The anonymous adapter action</param>
    /// <typeparam name="TData">The data type expected by the adapter</typeparam>
    public static IChannelBuilder AddOutputAdapter<TData>( this IChannelBuilder builder, Action<IServiceProvider, IAdapterContext, TData> action )
    {
        builder.AddOutputAdapter( ( provider ) =>
        {
            return new AnonymousChannelAdapter<TData>( async ( context, data ) =>
            {
                await Task.Run( () => action.Invoke( provider, context, data ) )
                    .ConfigureAwait( false );
            } );
        } );

        return builder;
    }
}
