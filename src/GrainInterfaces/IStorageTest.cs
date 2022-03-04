namespace GrainInterfaces;

using Orleans;

public interface IStorageTest : IGrainWithGuidKey
{
    Task<int> IncreaseCount();
}