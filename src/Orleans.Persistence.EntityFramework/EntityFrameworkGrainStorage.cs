namespace Orleans.Persistence.EntityFramework;

using Orleans.Runtime;
using Orleans.Storage;

public class EntityFrameworkGrainStorage : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
{
    readonly string storageName;
    readonly IServiceProvider services;

    public EntityFrameworkGrainStorage(string storageName, IServiceProvider services)
    {
        this.storageName = storageName;
        this.services = services;
    }

    public static EntityFrameworkGrainStorage Create(IServiceProvider services, string name)
    {
        return new EntityFrameworkGrainStorage(name, services);
    }

    public async Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
    {
        var repo = services.GetRequiredServiceByName<IRepositoryCore>(grainState.State.GetType().Name);
        var result = await repo.ReadAsync(grainReference.GrainIdentity.PrimaryKey, CancellationToken.None);

        if (result != null)
        {
            grainState.State = result;
        }
    }

    public async Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
    {
        var repo = services.GetRequiredServiceByName<IRepositoryCore>(grainState.State.GetType().Name);
        var current = await repo.ReadAsync(grainReference.GrainIdentity.PrimaryKey, CancellationToken.None);
        if (current == null)
        {
            await repo.AddAsync(grainState.State, CancellationToken.None);
        }
        else
        {
            await repo.UpdateAsync(grainReference.GrainIdentity.PrimaryKey, grainState.State, CancellationToken.None);
        }
    }

    public async Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
    {
        var repo = services.GetRequiredServiceByName<IRepositoryCore>(grainState.State.GetType().Name);
        await repo.DeleteAsync(grainState.State, CancellationToken.None);
    }

    public void Participate(ISiloLifecycle lifecycle)
    {
        lifecycle.Subscribe(OptionFormattingUtilities.Name<EntityFrameworkGrainStorage>(storageName),
                            ServiceLifecycleStage.ApplicationServices,
                            Init);
    }

    Task Init(CancellationToken ct)
    {
        // maybe we need to initialize some EF stuff here?
        return Task.CompletedTask;
    }
}