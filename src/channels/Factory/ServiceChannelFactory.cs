using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Faactory.Channels.Adapters;
using Faactory.Channels.Hosting;
using Faactory.Sockets;

namespace Faactory.Channels;

internal class ServiceChannelFactory : IServiceChannelFactory
{
    private readonly IServiceProvider serviceProvider;
    private readonly ServiceChannelOptions options;

    public ServiceChannelFactory( IServiceProvider serviceProvider
        , IOptions<ServiceChannelOptions> optionsAccessor )
    {
        this.serviceProvider = serviceProvider;

        options = optionsAccessor.Value;
    }

    public IChannel CreateChannel( System.Net.Sockets.Socket socket )
    {
        var serviceScope = serviceProvider.CreateScope();
        var inputAdapters = serviceScope.ServiceProvider.GetAdapters<IInputChannelAdapter>();
        var outputAdapters = serviceScope.ServiceProvider.GetAdapters<IOutputChannelAdapter>();
        var inputHandlers = serviceScope.ServiceProvider.GetHandlers();

        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

        var idleChannelMonitor = ( options.IdleDetectionMode > IdleDetectionMode.None )
            ? new IdleChannelMonitor( loggerFactory, options.IdleDetectionMode, options.IdleDetectionTimeout )
            : null;

        var channel = new ServiceChannel( serviceScope
            , loggerFactory
            , socket
            , inputAdapters
            , outputAdapters
            , inputHandlers
            , idleChannelMonitor );

        channel.BeginReceive();

        return ( channel );
    }
}
