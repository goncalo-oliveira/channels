using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Faactory.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace tests;

internal sealed class TestServer( IHost host, IServiceProvider serviceProvider, int port ) : IAsyncDisposable
{
    public IHost Application { get; } = host;
    public int Port { get; } = port;
    public IServiceProvider Services { get; } = serviceProvider;

    public async Task StopAsync( CancellationToken cancellationToken = default )
    {
        await Application.StopAsync( cancellationToken ).ConfigureAwait( false );
    }

    public static async Task<TestServer> CreateAsync( Action<IChannelBuilder>? configureChannel = null, Action<IServiceCollection>? configureServices = null, CancellationToken cancellationToken = default )
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Services.AddLogging();
        builder.Services.AddChannels( options => configureChannel?.Invoke( options ) );

        builder.Services.AddTcpChannelListener( options =>
        {
            options.Port = 0; // dynamic
            options.Backlog = 1;
        } );

        configureServices?.Invoke( builder.Services );

        var app = builder.Build();

        await app.StartAsync( cancellationToken ).ConfigureAwait( false );

        return new TestServer( app, app.Services, GetBoundTcpPort( app.Services ) );
    }

    private static int GetBoundTcpPort(IServiceProvider services)
    {
        // TcpListener is registered as IHostedService. Find it.
        var hosted = services.GetServices<IHostedService>();
        var listener = hosted.OfType<Faactory.Channels.Tcp.TcpListener>().SingleOrDefault()
            ?? throw new InvalidOperationException( "TcpListener not found among IHostedService implementations." );

        // read private field "socket" and its LocalEndPoint
        var socketProperty = typeof( Faactory.Channels.Tcp.TcpListener )
            .GetProperty( "Socket", BindingFlags.NonPublic | BindingFlags.Instance )
                ?? throw new InvalidOperationException( "TcpListener.Socket property not found." );

        var socket = (System.Net.Sockets.Socket?)socketProperty!.GetValue( listener );
        var ep = (System.Net.IPEndPoint)socket!.LocalEndPoint!;

        return ep.Port;
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait( false );

        Application.Dispose();
    }
}
