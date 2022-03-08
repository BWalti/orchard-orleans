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
        services.AddDbContextFactory<TestDbContext>((services, builder) =>
        {
            var config = services.GetRequiredService<IConfiguration>();
            builder.UseNpgsql(config.GetConnectionString("TestContext"));
        });

        //services.AddMarten(options =>
        //{
        //    options.Connection(hostContext.Configuration.GetConnectionString("TestContext"));

        //    if (hostContext.HostingEnvironment.IsDevelopment())
        //    {
        //        options.AutoCreateSchemaObjects = AutoCreate.All;
        //    }
        //});

        services.AddRepository<CounterState, TestDbContext, Guid?>();
        services.AddRepository<EnergyConsumption, TestDbContext, long?>();
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

        builder.AddAdoNetGrainStorageAsDefault(options =>
        {
            options.Invariant = "Npgsql";
            options.ConnectionString = context.Configuration.GetConnectionString("TestContext");
        });

        builder.AddLogStorageBasedLogConsistencyProviderAsDefault();
        //builder.AddCustomStorageBasedLogConsistencyProviderAsDefault();

        builder
            .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(HelloGrain).Assembly).WithReferences())
            .ConfigureLogging(logging => logging.AddConsole());

        if (context.HostingEnvironment.IsDevelopment())
        {
            builder.UseDashboard();

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
await ProofOfConcepts(app.Services);
await app.WaitForShutdownAsync();

async Task ProofOfConcepts(IServiceProvider serviceProvider)
{
    var factory = serviceProvider.GetRequiredService<IGrainFactory>();
    //var grain = factory.GetGrain<ISmartMeterOverviewGrain>(Guid.Parse("0ea2271b-0f80-4534-9421-941fcd4b4f87"));

    //var state = await grain.GetState();
    //await grain.AddMeasurement(100);
    //state = await grain.GetState();

    var init = factory.GetGrain<IStorageTest>(Guid.Parse("0ea2271b-0f80-4534-9421-941fcd4b4f87"));
    var result = await init.IncreaseCount();

    //var smartMeterIds = Enumerable.Range(1, 5).ToArray();
    //var smartMeterings = Task.Factory.StartNew(async () =>
    //{
    //    var grains = smartMeterIds.Select(id => factory.GetGrain<ISmartMeterGrain>(id)).ToList();
    //    while (true)
    //    {
    //        foreach (var meterGrain in grains)
    //        {
    //            await meterGrain.IncrementUsage(Random.Shared.Next(50, 300));
    //        }

    //        await Task.Delay(TimeSpan.FromMilliseconds(500));
    //    }
    //});

    //var readOuts = Task.Factory.StartNew(async () =>
    //{
    //    var grains = smartMeterIds.Select(id => factory.GetGrain<ISmartMeterGrain>(id)).ToList();
    //    while (true)
    //    {
    //        int overallUsage = 0;
    //        foreach (var meterGrain in grains)
    //        {
    //            overallUsage += await meterGrain.OverallUsage();
    //        }

    //        Console.WriteLine($"=== Overall Usage: {overallUsage} kWh");
    //        await Task.Delay(TimeSpan.FromSeconds(5));
    //    }
    //});
}