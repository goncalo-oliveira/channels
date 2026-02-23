using System.Diagnostics.Metrics;

namespace Faactory.Channels;

internal static class Metrics
{
    private static readonly Meter Meter = new( new MeterOptions( "channels" )
    {
        Version = typeof( Metrics ).Assembly.GetName().Version?.ToString()
    } );

    public static readonly Counter<long> BytesReceived =
        Meter.CreateCounter<long>( "channels.bytes_received", "bytes", "Total number of bytes received" );

    public static readonly Counter<long> BytesSent =
        Meter.CreateCounter<long>( "channels.bytes_sent", "bytes", "Total number of bytes sent" );

    public static readonly UpDownCounter<long> ActiveChannels =
        Meter.CreateUpDownCounter<long>( "channels.active", description: "Number of active channels" );

    public static readonly Histogram<double> ChannelDuration =
        Meter.CreateHistogram<double>( "channels.lifetime_duration", "ms", "Channel lifetime duration in milliseconds" );

    public static readonly Counter<long> MiddlewareExceptions =
        Meter.CreateCounter<long>( "channels.middleware_exceptions", description: "Total number of middleware exceptions" );

    public static readonly Counter<long> IdleTimeouts =
        Meter.CreateCounter<long>( "channels.idle_timeouts", description: "Total number of idle timeouts" );
}
