namespace Grains;

using GrainInterfaces;

using Orleans;
using Orleans.Runtime;

public class StorageTestGrain : Grain, IStorageTest
{
    readonly IPersistentState<CounterState> state;

    public StorageTestGrain([PersistentState("counter", "EF")] IPersistentState<CounterState> state)
    {
        this.state = state;
    }

    public async Task<int> IncreaseCount()
    {
        var result = state.State.Counter++;
        await state.WriteStateAsync();
        return result;
    }
}

public class CounterState
{
    public Guid Id { get; set; }

    public int Counter { get; set; }
}