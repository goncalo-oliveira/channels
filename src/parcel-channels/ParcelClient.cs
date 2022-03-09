// using System.Net;
// using System.Net.Sockets;
// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Options;
// using Parcel.Channels;

// namespace Parcel.Sockets;

// public class ParcelClient
// {
//     private readonly ILoggerFactory loggerFactory;

//     private ClientChannel? channel;

//     public ParcelClient()
//     {
//         loggerFactory = new LoggerFactory();
//     }

//     public ParcelClient( ILoggerFactory loggerFactory )
//     {
//         this.loggerFactory = loggerFactory;
//     }

//     public async Task<ClientChannel> ConnectAsync( string host, int port, CancellationToken cancellationToken = default( CancellationToken ) )
//     {
//         // create a TCP/IP socket
//         var client = new Socket( SocketType.Stream, ProtocolType.Tcp );

//         await client.ConnectAsync( host, port, cancellationToken );

//         channel = new ClientChannel( loggerFactory
//             , client
//             , IdleDetectionMode.Auto
//             , TimeSpan.FromSeconds( 60 ) );

//         return ( channel );
//     }

//     public void Disconnect()
//     {
//         if ( channel == null )
//         {
//             return;
//         }

//         try
//         {
//             channel.Socket.Shutdown( SocketShutdown.Both );
//             channel.Socket.Disconnect( false );
//             channel.Socket.TryClose();
//         }
//         catch ( Exception )
//         {}
//         finally
//         {
//             channel = null;
//         }
//     }

//     public Task SendAsync( byte[] data )
//     {
//         if ( channel == null )
//         {
//             return Task.CompletedTask;
//         }

//         return Task.Run( () => channel.Send( data ) );
//     }
// }
