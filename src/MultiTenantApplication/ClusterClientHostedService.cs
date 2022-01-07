using System;
using System.Threading;
using System.Threading.Tasks;
using GrainInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Runtime;

namespace MultiTenantApplication;

public class ClusterClientHostedService : IHostedService
{
    private readonly ILogger<ClusterClientHostedService> logger;

    public ClusterClientHostedService(ILogger<ClusterClientHostedService> logger, ILoggerProvider loggerProvider,
        IConfiguration config)
    {
        this.logger = logger;
        Client = new ClientBuilder()
            .ConfigureAppConfiguration(builder => builder.AddConfiguration(config))
            .UseLocalhostClustering()
            .Configure<ClusterOptions>(options =>
            {
                options.ClusterId = "dev";
                options.ServiceId = "OrleansBasics";
            })
            .ConfigureApplicationParts(parts => { parts.AddApplicationPart(typeof(IHello).Assembly).WithReferences(); })
            .ConfigureLogging(builder => builder.AddProvider(loggerProvider))
            .Build();
    }

    public IClusterClient Client { get; }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var attempt = 0;
        var maxAttempts = 100;
        var delay = TimeSpan.FromSeconds(1);
        return Client.Connect(async error =>
        {
            if (cancellationToken.IsCancellationRequested) return false;

            if (++attempt < maxAttempts)
            {
                logger.LogWarning(error,
                    "Failed to connect to Orleans cluster on attempt {@Attempt} of {@MaxAttempts}.",
                    attempt, maxAttempts);

                try
                {
                    await Task.Delay(delay, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    return false;
                }

                return true;
            }

            logger.LogError(error,
                "Failed to connect to Orleans cluster on attempt {@Attempt} of {@MaxAttempts}.",
                attempt, maxAttempts);

            return false;
        });
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Client.Close();
        }
        catch (OrleansException error)
        {
            logger.LogWarning(error,
                "Error while gracefully disconnecting from Orleans cluster. Will ignore and continue to shutdown.");
        }
    }
}