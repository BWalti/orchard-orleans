using Orleans.Persistence.EntityFramework;

namespace Grains;

using GrainInterfaces;

using Orleans;
using Orleans.Runtime;

public class StorageTestGrain : Grain, IStorageTest
{
    readonly IPersistentState<CounterState> state;

    public StorageTestGrain([PersistentState("counter", EntityFrameworkGrainStorage.DefaultProviderName)] IPersistentState<CounterState> state)
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

public class CounterState : EfPersistableState<Guid?>
{
    public int Counter { get; set; }
}