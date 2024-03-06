using System;
using System.Threading;
using System.Threading.Tasks;
using Faactory.Channels;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Faactory.Channels.Tests;

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

        var channel = new DetachedChannel();

        var service = provider.GetRequiredService<IdleChannelService>();

        await service.StartAsync( channel, CancellationToken.None );

        await Task.Delay( TimeSpan.FromSeconds( 1.5 ) );

        await service.StopAsync( CancellationToken.None );

        Assert.True( channel.IsClosed );
    }
}
