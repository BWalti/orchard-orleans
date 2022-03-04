using System.Diagnostics;

using GrainInterfaces;

using Grains;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Clustering.Kubernetes;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Persistence.EntityFramework;
using Orleans.Runtime;
using Orleans.Storage;

using Silo;

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
        //services.AddDbContext<TestDbContext>((services, options) =>
        //{
        //    var config = services.GetRequiredService<IConfiguration>();
        //    options.UseNpgsql(config.GetConnectionString("TestContext"));
        //});
        services.AddDbContextFactory<TestDbContext>((services, builder) =>
        {
            var config = services.GetRequiredService<IConfiguration>();
            builder.UseNpgsql(config.GetConnectionString("TestContext"));
        });

        services.AddSingletonNamedService<IRepositoryCore, RepositoryCore<CounterState, Guid>>(nameof(CounterState));
        services.AddSingletonNamedService<IRepository<CounterState, Guid>, Repository<CounterState, Guid, TestDbContext>>(nameof(CounterState));

        //services.AddSingleton<IQueueWorkerClient, QueueWorkerClient>();
    })
    .UseOrleans((context, builder) =>
    {
        builder.ConfigureServices(collection =>
        {
            collection.AddSingletonNamedService<IGrainStorage>("EF", EntityFrameworkGrainStorage.Create)
                      .AddSingletonNamedService("EF",
                                                (s, n) => (ILifecycleParticipant<ISiloLifecycle>)s
                                                    .GetRequiredServiceByName<IGrainStorage>(n));
        });

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

// docker run --name orleans-demo -p 5432:5432 -e POSTGRES_PASSWORD=secretPassword -e POSTGRES_USER=demo -e POSTGRES_DB=demo -d postgres
var factory = app.Services.GetRequiredService<IGrainFactory>();
var init = factory.GetGrain<IStorageTest>(Guid.Parse("0ea2271b-0f80-4534-9421-941fcd4b4f87"));
var result = await init.IncreaseCount();

await app.WaitForShutdownAsync();