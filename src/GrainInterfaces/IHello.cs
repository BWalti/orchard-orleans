using Orleans;

namespace GrainInterfaces;

public interface IHello : IGrainWithStringKey
{
    Task<string> SayHello(string greeting);
}