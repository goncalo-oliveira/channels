using Faactory.Channels;

namespace Microsoft.Extensions.DependencyInjection;

public static class ChannelClientFactoryServiceExtensions
{
    /// <summary>
    /// Adds the channel client factory to the specified service IServiceCollection
    /// </summary>
    public static IServiceCollection AddClientChannelFactory( this IServiceCollection services, Action<IClientChannelBuilder> configure )
    {
        services.AddTransient<IClientChannelFactory, ClientChannelFactory>();

        var builder = new ClientChannelBuilder( services );

        configure( builder );

        return ( services );
    }
}
