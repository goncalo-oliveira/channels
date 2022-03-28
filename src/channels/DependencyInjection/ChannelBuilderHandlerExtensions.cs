using Microsoft.Extensions.DependencyInjection;
using Faactory.Channels.Handlers;

namespace Faactory.Channels;

public static class ChannelBuilderHandlerExtensions
{
    /// <summary>
    /// Adds a transient service for the channel handler to the input pipeline
    /// </summary>
    /// <typeparam name="TAdapter">The type of the handler implementation</typeparam>
    public static IChannelBuilder AddInputHandler<THandler>( this IChannelBuilder builder ) where THandler : class, IChannelHandler
    {
        builder.Services.AddTransient<IChannelHandler, THandler>();

        return ( builder );
    }
}
