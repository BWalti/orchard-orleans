using System.Globalization;
using Orleans.Runtime;
using Orleans.Storage;

namespace Orleans.Persistence.EntityFramework;

public class EntityFrameworkGrainStorage : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
{
    public const string DefaultProviderName = nameof(EntityFrameworkGrainStorage);

    readonly IServiceProvider _services;
    readonly string _storageName;

    public EntityFrameworkGrainStorage(string storageName, IServiceProvider services)
    {
        _storageName = storageName;
        _services = services;
    }

    public async Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
    {
        CheckStateImplementsIProvideETag(grainState, grainReference, out _, out var key);

        var repo = _services.GetRequiredServiceByName<IRepositoryCore>(grainState.State.GetType().Name);
        var result = await repo.ReadAsync(key, CancellationToken.None);

        if (result != null)
        {
            grainState.State = result;
            grainState.RecordExists = true;
            grainState.ETag = result.ETag.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            grainState.ETag = "0";
        }
    }

    public async Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
    {
        CheckStateImplementsIProvideETag(grainState, grainReference, out var stateWithETag, out var key);

        var repo = _services.GetRequiredServiceByName<IRepositoryCore>(stateWithETag.GetType().Name);
        if (grainState.RecordExists)
        {
            await repo.UpdateAsync(key, stateWithETag, CancellationToken.None);
        }
        else
        {
            // todo: what if the same "state" type gets loaded from another grain? do we want to share?
            switch (stateWithETag)
            {
                case EfPersistableState<Guid?> guidId:
                    guidId.Id = grainReference.GrainIdentity.PrimaryKey;
                    guidId.ETag = 1;
                    break;

                case EfPersistableState<long?> longId:
                    longId.Id = grainReference.GrainIdentity.PrimaryKeyLong;
                    longId.ETag = 1;
                    break;

                case EfPersistableState<string?> stringId:
                    stringId.Id = grainReference.GrainIdentity.IdentityString;
                    stringId.ETag = 1;
                    break;

                default:
                    throw new InvalidOperationException(
                        $"EF persistable grain state needs to extend one of: {nameof(EfPersistableState<Guid?>)}, {nameof(EfPersistableState<long?>)}, {nameof(EfPersistableState<string?>)}");
            }

            await repo.AddAsync(stateWithETag, CancellationToken.None);
        }
    }

    public async Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
    {
        CheckStateImplementsIProvideETag(grainState, grainReference, out var stateWithETag, out _);

        var repo = _services.GetRequiredServiceByName<IRepositoryCore>(grainState.State.GetType().Name);
        await repo.DeleteAsync(stateWithETag, CancellationToken.None);
    }

    public void Participate(ISiloLifecycle lifecycle) =>
        lifecycle.Subscribe(OptionFormattingUtilities.Name<EntityFrameworkGrainStorage>(_storageName),
            ServiceLifecycleStage.ApplicationServices,
            Init);

    Task Init(CancellationToken ct) =>
        // maybe we need to initialize some EF stuff here?
        Task.CompletedTask;

    public static EntityFrameworkGrainStorage Create(IServiceProvider services, string name) => new(name, services);

    static void CheckStateImplementsIProvideETag(IGrainState grainState, GrainReference grainReference, out IProvideETag stateWithETag, out object key)
    {
        if (grainState.State is not IProvideETag tag)
            throw new InvalidOperationException(
                $"Only State implementing {nameof(IProvideETag)} can be persisted using {nameof(EntityFrameworkGrainStorage)} provider");

        stateWithETag = tag;

        // new!
        key = stateWithETag switch
        {
            EfPersistableState<Guid?> => grainReference.GrainIdentity.PrimaryKey,
            EfPersistableState<long?> => grainReference.GrainIdentity.PrimaryKeyLong,
            EfPersistableState<string?> => grainReference.GrainIdentity.IdentityString,

            _ => throw new InvalidOperationException(
                $"EF persistable grain state needs to extend one of: {nameof(EfPersistableState<Guid?>)}, {nameof(EfPersistableState<long?>)}, {nameof(EfPersistableState<string?>)}")
        };
    }
}

//public class UnicornJournaledGrain<TGrainState, TEventBase> : 
//    JournaledGrain<TGrainState, TEventBase>, 
//    ICustomStorageInterface<TGrainState, TEventBase>

//    where TGrainState : class, new() 
//    where TEventBase : class
//{
//    public async Task<KeyValuePair<int, TGrainState>> ReadStateFromStorage()
//    {
//        var grainStorage = ServiceProvider.GetRequiredServiceByName<IGrainStorage>(EntityFrameworkGrainStorage.DefaultProviderName);

//        var grainState = new GrainState<TGrainState>(State);
//        await grainStorage.ReadStateAsync(typeof(TGrainState).Name, GrainReference, grainState);

//        return KeyValuePair.Create(int.Parse(grainState.ETag), grainState.State);
//    }

//    public async Task<bool> ApplyUpdatesToStorage(IReadOnlyList<TEventBase> updates, int expectedversion)
//    {
//        var grainStorage = ServiceProvider.GetRequiredServiceByName<IGrainStorage>(EntityFrameworkGrainStorage.DefaultProviderName);

//        var grainState = new GrainState<TGrainState>(State);
//        await grainStorage.WriteStateAsync(typeof(TGrainState).Name, GrainReference, grainState);
//    }
//}