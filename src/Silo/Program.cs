using Grains;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

var host = Host.CreateDefaultBuilder(args)
    //.ConfigureAppConfiguration(builder =>
    //{
    //})
    //.ConfigureServices((hostContext, services) =>
    //{
    //})
    .UseOrleans((context, builder) =>
    {
        builder.UseDashboard();

        builder
            .UseLocalhostClustering()
            .Configure<ClusterOptions>(options =>
            {
                options.ClusterId = "dev";
                options.ServiceId = "OrleansBasics";
            })
            .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(HelloGrain).Assembly).WithReferences())
            .ConfigureLogging(logging => logging.AddConsole());
    });

await host.RunConsoleAsync();
