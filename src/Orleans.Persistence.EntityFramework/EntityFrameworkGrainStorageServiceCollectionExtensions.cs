using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Runtime;
using Orleans.Storage;

namespace Orleans.Persistence.EntityFramework;

public static class EntityFrameworkGrainStorageServiceCollectionExtensions
{
    public static IOrleansEntityFrameworkGrainStorage<TDbContext> AddOrleansEntityFrameworkGrainStorage<TDbContext>(this IServiceCollection services, string providerName = EntityFrameworkGrainStorage.DefaultProviderName)
    where TDbContext : DbContext
    {
        services.AddSingletonNamedService<IGrainStorage>(providerName, EntityFrameworkGrainStorage.Create)
            .AddSingletonNamedService(providerName,
                (s, n) => (ILifecycleParticipant<ISiloLifecycle>)s
                    .GetRequiredServiceByName<IGrainStorage>(n));

        return new OrleansEntityFrameworkGrainStorage<TDbContext>(services);
    }
}

public interface IOrleansEntityFrameworkGrainStorage<TDbContext>
    where TDbContext : DbContext
{
    IOrleansEntityFrameworkGrainStorage<TDbContext> AddRepository<TGrainState, TPrimaryKey>()
        where TGrainState : EfPersistableState<TPrimaryKey>, IProvideETag;
}

public class OrleansEntityFrameworkGrainStorage<TDbContext> : IOrleansEntityFrameworkGrainStorage<TDbContext>
    where TDbContext : DbContext
{
    public IServiceCollection Services { get; }
    public IOrleansEntityFrameworkGrainStorage<TDbContext> AddRepository<TGrainState, TPrimaryKey>() where TGrainState : EfPersistableState<TPrimaryKey>, IProvideETag
    {
        var stateName = typeof(TGrainState).Name;

        Services.AddSingletonNamedService<IRepositoryCore, RepositoryCore<TGrainState, TPrimaryKey>>(stateName);
        Services.AddSingletonNamedService<IRepository<TGrainState, TPrimaryKey>, Repository<TGrainState, TPrimaryKey, TDbContext>>(stateName);

        return this;
    }

    public OrleansEntityFrameworkGrainStorage(IServiceCollection serviceCollection)
    {
        Services = serviceCollection;
    }
}