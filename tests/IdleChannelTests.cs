using System;
using System.Collections;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Faactory.Channels;
using Faactory.Channels.Adapters;
using Faactory.Channels.Buffers;
using Faactory.Channels.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class IdleChannelServiceTests
{

    [Fact]
    public async Task TestIdleServiceAsync()
    {
        IServiceCollection services = new ServiceCollection()
            .AddLogging()
            .AddTransient<IdleChannelService>()
            .Configure<IdleChannelServiceOptions>( options =>
            {
                options.DetectionMode = IdleDetectionMode.Both;
                options.Timeout = TimeSpan.FromSeconds( 1 );
            } );

        var provider = services.BuildServiceProvider();

        var channel = new FakeChannel();

        var service = provider.GetRequiredService<IdleChannelService>();

        await service.StartAsync( channel, CancellationToken.None );

        await Task.Delay( TimeSpan.FromSeconds( 1.5 ) );

        await service.StopAsync( CancellationToken.None );

        Assert.True( channel.IsClosed );
    }
}
