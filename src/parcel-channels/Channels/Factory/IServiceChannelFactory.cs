namespace Parcel.Channels;

internal interface IServiceChannelFactory
{
    IChannel CreateChannel( System.Net.Sockets.Socket socket );
}
