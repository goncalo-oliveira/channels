using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Faactory.Channels;

internal class ServiceChannelFactory( IServiceProvider serviceProvider, IOptions<ServiceChannelOptions> optionsAccessor ) : IServiceChannelFactory
{
    private readonly IServiceProvider serviceProvider = serviceProvider;
    private readonly ServiceChannelOptions options = optionsAccessor.Value;

    public async Task<IChannel> CreateChannelAsync( System.Net.Sockets.Socket socket )
    {
        var serviceScope = serviceProvider.CreateScope();

        var channel = new ServiceChannel(
              serviceScope
            , socket
            , options.BufferEndianness );

        await channel.InitializeAsync();

        channel.BeginReceive();

        return ( channel );
    }
}
