using Faactory.Channels.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

public static class AnonymousHandlerChannelBuilderExtensions
{
    /// <summary>
    /// Adds a transient service for the anonymous channel handler to the input pipeline
    /// </summary>
    /// <param name="action">The anonymous handler action</param>
    /// <typeparam name="TData">The data type expected by the handler</typeparam>
    public static IChannelBuilder AddInputHandler<TData>( this IChannelBuilder builder, Action<IChannelContext, TData> action )
    {
        builder.AddInputHandler( provider =>
        {
            return new AnonymousChannelHandler<TData>( async ( context, data ) =>
            {
                await Task.Run( () => action.Invoke( context, data ) )
                    .ConfigureAwait( false );
            } );
        });

        return builder;
    }

    /// <summary>
    /// Adds a transient service for the anonymous channel handler to the input pipeline
    /// </summary>
    /// <param name="action">The anonymous handler action</param>
    /// <typeparam name="TData">The data type expected by the handler</typeparam>
    public static IChannelBuilder AddInputHandler<TData>( this IChannelBuilder builder, Action<IServiceProvider, IChannelContext, TData> action )
    {
        builder.AddInputHandler( provider =>
        {
            return new AnonymousChannelHandler<TData>( async ( context, data ) =>
            {
                await Task.Run( () => action.Invoke( provider, context, data ) )
                    .ConfigureAwait( false );
            } );
        } );

        return builder;
    }
}
