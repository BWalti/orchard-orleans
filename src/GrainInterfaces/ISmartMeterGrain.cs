using Orleans;

namespace GrainInterfaces;

public interface ISmartMeterGrain : IGrainWithIntegerKey
{
    Task IncrementUsage(int kWh);

    Task<int> OverallUsage();
}

public interface ISmartMeterOverviewGrain : IGrainWithGuidKey
{
    Task AddMeasurement(int kWh);
    Task<SmartMeterOverviewState> GetState();
}


public class SmartMeterOverviewState
{
    public int AmountOfMeasurements { get; set; }

    public int OverallConsumption { get; set; }

    public int MinConsumptionPerPeriod { get; set; }

    public int MaxConsumptionPerPeriod { get; set; }
}