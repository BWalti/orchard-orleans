using GrainInterfaces;
using Orleans;
using Orleans.Persistence.EntityFramework;
using Orleans.Runtime;

namespace Grains;

public class SmartMeterGrain : Grain, ISmartMeterGrain
{
    readonly IPersistentState<EnergyConsumption> _state;

    public SmartMeterGrain(
        [PersistentState("energyConsumption", "EF")] IPersistentState<EnergyConsumption> state
    )
    {
        _state = state;
    }

    public async Task IncrementUsage(int kWh)
    {
        _state.State.KwH += kWh;

        await _state.WriteStateAsync();
    }

    public Task<int> OverallUsage()
    {
        return Task.FromResult(_state.State.KwH);
    }
}

public class EnergyConsumption : EfPersistableState<long?>
{
    public int KwH { get; set; }
}