using Orleans.Runtime;

namespace Orleans.Persistence.EntityFramework;

public class RepositoryCore<TEntity, TPrimaryKey> : IRepositoryCore
    where TEntity : class, IProvideETag
{
    readonly IRepository<TEntity, TPrimaryKey> innerRepository;

    public RepositoryCore(IServiceProvider services)
    {
        innerRepository = services.GetRequiredServiceByName<IRepository<TEntity, TPrimaryKey>>(typeof(TEntity).Name);
    }

    public async Task<IProvideETag?> ReadAsync(object key, CancellationToken cancellationToken)
    {
        var result = await innerRepository.ReadAsync((TPrimaryKey)key, cancellationToken);
        return result;
    }

    public async Task<IProvideETag> AddAsync(IProvideETag entity, CancellationToken cancellationToken)
    {
        var result = await innerRepository.AddAsync((TEntity)entity, cancellationToken);
        return result;
    }

    public async Task<IProvideETag> UpdateAsync(object key, IProvideETag entity, CancellationToken cancellationToken)
    {
        var result = await innerRepository.UpdateAsync((TPrimaryKey)key, (TEntity)entity, cancellationToken);
        return result;
    }

    public async Task DeleteAsync(IProvideETag entity, CancellationToken cancellationToken)
    {
        await innerRepository.DeleteAsync((TEntity)entity, cancellationToken);
    }
}