using System.Diagnostics;
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
using Silo;

Activity.DefaultIdFormat = ActivityIdFormat.W3C;

var app = Host
    .CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddDbContextFactory<TestDbContext>(builder =>
        {
            builder.UseNpgsql(hostContext.Configuration.GetConnectionString("DbContext"));
        });

        // register Entity Framework Storage and all EF persistable grain state:
        services
            .AddOrleansEntityFrameworkGrainStorage<TestDbContext>()
            .AddRepository<CounterState, Guid?>()
            .AddRepository<EnergyConsumption, long?>();
    })
    .UseOrleans((context, builder) =>
    {
        builder.AddLogStorageBasedLogConsistencyProviderAsDefault();
        // WIP: maybe we don't need this anyway:
        //builder.AddCustomStorageBasedLogConsistencyProviderAsDefault();

        builder
            .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(HelloGrain).Assembly).WithReferences())
            .ConfigureLogging(logging => logging.AddConsole());

        if (context.HostingEnvironment.IsDevelopment())
        {
            builder.UseDashboard();

            // backup "default" storage, should only be used during development
            // as performance might not be production ready:
            builder.AddAdoNetGrainStorageAsDefault(options =>
            {
                options.Invariant = "Npgsql";
                options.ConnectionString = context.Configuration.GetConnectionString("DbContext");
            });

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

var environment = app.Services.GetRequiredService<IHostEnvironment>();
if (environment.IsDevelopment())
{
    // during development we might auto migrate the database:
    using var scope = app.Services.CreateScope();
    await using var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
    await dbContext.Database.MigrateAsync();
}

await app.StartAsync();
//await ProofOfConcepts(app.Services);
await app.WaitForShutdownAsync();

//async Task ProofOfConcepts(IServiceProvider serviceProvider)
//{
//    var factory = serviceProvider.GetRequiredService<IGrainFactory>();

//    //var grain = factory.GetGrain<ISmartMeterOverviewGrain>(Guid.Parse("0ea2271b-0f80-4534-9421-941fcd4b4f87"));
//    //await grain.AddMeasurement(100);

//    //var state = await grain.GetState();
//    //await grain.AddMeasurement(100);
//    //state = await grain.GetState();

//    //var init = factory.GetGrain<IStorageTest>(Guid.Parse("0ea2271b-0f80-4534-9421-941fcd4b4f87"));
//    //var result = await init.IncreaseCount();

//    //var smartMeterIds = Enumerable.Range(1, 5).ToArray();
//    //var smartMeterings = Task.Factory.StartNew(async () =>
//    //{
//    //    var grains = smartMeterIds.Select(id => factory.GetGrain<ISmartMeterGrain>(id)).ToList();
//    //    while (true)
//    //    {
//    //        foreach (var meterGrain in grains)
//    //        {
//    //            await meterGrain.IncrementUsage(Random.Shared.Next(50, 300));
//    //        }

//    //        await Task.Delay(TimeSpan.FromMilliseconds(500));
//    //    }
//    //});

//    //var readOuts = Task.Factory.StartNew(async () =>
//    //{
//    //    var grains = smartMeterIds.Select(id => factory.GetGrain<ISmartMeterGrain>(id)).ToList();
//    //    while (true)
//    //    {
//    //        int overallUsage = 0;
//    //        foreach (var meterGrain in grains)
//    //        {
//    //            overallUsage += await meterGrain.OverallUsage();
//    //        }

//    //        Console.WriteLine($"=== Overall Usage: {overallUsage} kWh");
//    //        await Task.Delay(TimeSpan.FromSeconds(5));
//    //    }
//    //});
//}