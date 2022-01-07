using GrainInterfaces;
using Microsoft.Extensions.Logging;
using Orleans;

namespace Grains;

public class HelloGrain : Grain, IHello
{
    private readonly ILogger logger;

    public HelloGrain(ILogger<HelloGrain> logger)
    {
        this.logger = logger;
    }

    public Task<string> SayHello(string greeting)
    {
        logger.LogInformation($"\n SayHello message received: greeting = '{greeting}'");
        return Task.FromResult($"Hello {greeting}");
    }
}