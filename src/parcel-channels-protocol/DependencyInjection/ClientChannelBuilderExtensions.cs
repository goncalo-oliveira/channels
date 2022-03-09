using Microsoft.Extensions.DependencyInjection;
using Parcel.Channels.Observables;

namespace Parcel.Channels;

public static class ClientChannelBuilderExtensions
{
    /// <summary>
    /// Adds a singleton service for the message observer
    /// </summary>
    public static IClientChannelBuilder AddMessageObserver( this IClientChannelBuilder builder )
    {
        builder.Services.AddSingleton<IMessageObserver, MessageObserver>();

        return ( builder );
    }
}
