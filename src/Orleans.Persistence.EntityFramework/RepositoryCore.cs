using Orleans.Runtime;

namespace Orleans.Persistence.EntityFramework;

public class RepositoryCore<TEntity, TPrimaryKey> : IRepositoryCore
    where TEntity : class
{
    readonly IServiceProvider services;

    public RepositoryCore(IServiceProvider services)
    {
        this.services = services;
    }

    public async Task<object> ReadAsync(object key, CancellationToken cancellationToken)
    {
        var result = await GetTypedRepository().ReadAsync((TPrimaryKey)key, cancellationToken);
        return result;
    }

    public async Task<object> AddAsync(object entity, CancellationToken cancellationToken)
    {
        var result = await GetTypedRepository().AddAsync((TEntity)entity, cancellationToken);
        return result;
    }

    public async Task<object> UpdateAsync(object key, object entity, CancellationToken cancellationToken)
    {
        var result = await GetTypedRepository().UpdateAsync((TPrimaryKey)key, (TEntity)entity, cancellationToken);
        return result;
    }

    public async Task DeleteAsync(object entity, CancellationToken cancellationToken)
    {
        await GetTypedRepository().DeleteAsync((TEntity)entity, cancellationToken);
    }

    private IRepository<TEntity, TPrimaryKey> GetTypedRepository()
    {
        return services.GetRequiredServiceByName<IRepository<TEntity, TPrimaryKey>>(typeof(TEntity).Name);
    }
}