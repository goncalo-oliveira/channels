using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = new HostBuilder()
    .ConfigureServices( ( context, services ) =>
    {
        // configure logging
        services.AddLogging( loggingBuilder =>
        {
            loggingBuilder.AddSimpleConsole( options =>
            {
                options.IncludeScopes = false;
                options.TimestampFormat = "hh:mm:ss";
            } )
            .SetMinimumLevel( LogLevel.Information );
        } );

        // add our hosted service
        services.AddChannelsHostedService( builder =>
        {
            // configure options
            builder.Configure( options =>
            {
                options.Port = 8080;
                options.Backlog = 10;
            } );

            // set up long-running services
            // since v0.5 idle monitoring is a channel service
            builder.AddIdleChannelService();

            // set up input pipeline
            /*
            We are replying the received data as it is, therefore we don't need adapters
            */
            builder.AddInputHandler<EchoHandler>();

            // set up output pipeline
            /*
            We are replying the received data as it is, therefore we don't need adapters
            */
        } );

    } )
    .UseConsoleLifetime( options =>
    {
        options.SuppressStatusMessages = true;
    } );

await builder.Build().RunAsync();
