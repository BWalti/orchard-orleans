using System.Threading.Tasks;
using GrainInterfaces;
using Microsoft.AspNetCore.Mvc;
using Orleans;

namespace Module1.Controllers;

[ApiController]
[Route("[controller]")]
public class SampleOrleansController : ControllerBase
{
    private readonly IClusterClient _orleansClient;

    public SampleOrleansController(IClusterClient orleansClient)
    {
        _orleansClient = orleansClient;
    }

    [HttpGet]
    public async Task<string> Get()
    {
        var helloGrain = this._orleansClient.GetGrain<IHello>("any-id");

        return await helloGrain.SayHello("World");
    }
}