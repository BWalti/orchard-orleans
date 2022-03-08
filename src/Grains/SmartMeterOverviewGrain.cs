using GrainInterfaces;
using Orleans.EventSourcing;

namespace Grains;

public class SmartMeterOverviewGrain : JournaledGrain<SmartMeterOverviewState, SmartMeterEvent>,
    ISmartMeterOverviewGrain
{
    public Task AddMeasurement(int kWh)
    {
        RaiseEvent(new SmartMeterMeasurementReceived(GrainReference.GrainIdentity.PrimaryKey, Guid.Empty, kWh));

        return ConfirmEvents();
    }

    public Task<SmartMeterOverviewState> GetState() => Task.FromResult(State);

    protected override void TransitionState(SmartMeterOverviewState state, SmartMeterEvent @event)
    {
        switch (@event)
        {
            case SmartMeterMeasurementReceived measurementReceived:
                state.AmountOfMeasurements++;
                state.MaxConsumptionPerPeriod =
                    Math.Max(State.MaxConsumptionPerPeriod, measurementReceived.Measurement);
                state.MinConsumptionPerPeriod =
                    Math.Min(State.MinConsumptionPerPeriod, measurementReceived.Measurement);
                state.OverallConsumption += measurementReceived.Measurement;

                break;

            default:
                throw new InvalidOperationException($"Can't handle message: {@event.GetType()}");
        }
    }
}

public abstract record SmartMeterEvent(Guid SmartMeterId, Guid TenantId);

public record SmartMeterMeasurementReceived(Guid SmartMeterId, Guid TenantId, int Measurement)
    : SmartMeterEvent(SmartMeterId, TenantId);