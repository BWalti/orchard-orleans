namespace Orleans.Persistence.EntityFramework;

public interface IRepository<TEntity, TPrimaryKey>
where TEntity : class, IProvideETag
{
    Task<TEntity?> ReadAsync(TPrimaryKey key, CancellationToken cancellationToken);

    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken);

    Task<TEntity> UpdateAsync(TPrimaryKey key, TEntity entity, CancellationToken cancellationToken);

    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken);
}