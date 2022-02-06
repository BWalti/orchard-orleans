using System.Diagnostics;
using Grains;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Clustering.Kubernetes;
using Orleans.Configuration;
using Orleans.Hosting;

static void ConfigureDelegate(IConfigurationBuilder configurationBuilder)
{
    configurationBuilder.AddJsonFile("appsettings.json");
}

Activity.DefaultIdFormat = ActivityIdFormat.W3C;

var app = Host
    .CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(ConfigureDelegate)
    .ConfigureAppConfiguration(ConfigureDelegate)
    .ConfigureServices((hostContext, services) =>
    {
        //services.AddSingleton<IQueueWorkerClient, QueueWorkerClient>();
    })
    .UseOrleans((context, builder) =>
    {
        builder.UseDashboard();

        builder
            .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(HelloGrain).Assembly).WithReferences())
            .ConfigureLogging(logging => logging.AddConsole());

        if (context.HostingEnvironment.IsDevelopment())
        {
            builder
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "OrleansBasics";
                })
                .UseLocalhostClustering();
        }
        else
        {
            builder.UseKubernetesHosting();
            builder.UseKubeMembership();
        }
    })
    .UseConsoleLifetime()
    .Build();

await app.StartAsync();
await app.WaitForShutdownAsync();