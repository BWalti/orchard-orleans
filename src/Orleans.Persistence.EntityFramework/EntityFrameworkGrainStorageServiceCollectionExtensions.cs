using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Runtime;

namespace Orleans.Persistence.EntityFramework;

public static class EntityFrameworkGrainStorageServiceCollectionExtensions
{
    public static IServiceCollection AddRepository<TGrainState, TDbContext, TPrimaryKey>(this IServiceCollection services)
    where TGrainState : EfPersistableState<TPrimaryKey>, IProvideETag
    where TDbContext : DbContext
    {
        var stateName = typeof(TGrainState).Name;

        services.AddSingletonNamedService<IRepositoryCore, RepositoryCore<TGrainState, TPrimaryKey>>(stateName);
        services.AddSingletonNamedService<IRepository<TGrainState, TPrimaryKey>, Repository<TGrainState, TPrimaryKey, TDbContext>>(stateName);

        return services;
    }
}