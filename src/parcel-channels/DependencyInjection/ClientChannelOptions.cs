namespace Parcel.Channels;

public class ClientChannelOptions : ChannelOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 8080;
}
