using Faactory.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder( args );

builder.Services.AddChannels( channel =>
{
    // set up input pipeline (echo server)
    channel.AddInputHandler<byte[]>( ( context, data ) =>
    {
        context.Output.Write( data );
    } );
} );

// set up TCP channel listener
builder.Services.AddTcpChannelListener( options =>
{
    options.Port = 8080;
    options.Backlog = 10;
} );

// set up metrics and exporter
// the easiest way to expose metrics is to use the built-in Kestrel web server
builder.WebHost.ConfigureKestrel( options =>
{
    options.ListenAnyIP( 9091 ); // most Prometheus exporters listen on port 9091 by default
} );

builder.Services.AddOpenTelemetry()
    .WithMetrics( metrics =>
    {
        // not required, but it's good practice to add resource information to your metrics
        metrics.ConfigureResource( r =>
        {
            r.AddService( "example", serviceVersion: typeof( Program ).Assembly.GetName().Version?.ToString() );
        });

        // add the channels meter to the OpenTelemetry pipeline
        metrics.AddMeter( "channels" );

        // add the Prometheus exporter to expose metrics in a format that Prometheus can scrape
        metrics.AddPrometheusExporter();
    } );

var app = builder.Build();

// map the Prometheus scraping endpoint to expose metrics at /metrics
app.MapPrometheusScrapingEndpoint();

app.Run();
