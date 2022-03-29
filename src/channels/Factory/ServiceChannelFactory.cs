using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Faactory.Channels.Adapters;
using Faactory.Channels.Hosting;
using Faactory.Sockets;

namespace Faactory.Channels;

internal class ServiceChannelFactory : ChannelFactory, IServiceChannelFactory
{
    private readonly ILoggerFactory loggerFactory;
    private readonly ServiceChannelOptions options;

    public ServiceChannelFactory( ILoggerFactory loggerFactory
        , IServiceProvider serviceProvider
        , IOptions<ServiceChannelOptions> optionsAccessor )
        : base( serviceProvider )
    {
        this.loggerFactory = loggerFactory;

        options = optionsAccessor.Value;
    }

    public IChannel CreateChannel( System.Net.Sockets.Socket socket )
    {
        var inputAdapters = GetAdapters<IInputChannelAdapter>();
        var outputAdapters = GetAdapters<IOutputChannelAdapter>();
        var inputHandlers = GetHandlers();

        var idleChannelMonitor = ( options.IdleDetectionMode > IdleDetectionMode.None )
            ? new IdleChannelMonitor( loggerFactory, options.IdleDetectionMode, options.IdleDetectionTimeout )
            : null;

        var channel = new ServiceChannel( loggerFactory
            , socket
            , inputAdapters
            , outputAdapters
            , inputHandlers
            , idleChannelMonitor );

        channel.BeginReceive();

        return ( channel );
    }
}
