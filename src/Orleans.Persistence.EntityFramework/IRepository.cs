namespace Orleans.Persistence.EntityFramework;

using Microsoft.EntityFrameworkCore;

using Orleans.Runtime;

public interface IRepositoryCore
{
    Task<object> ReadAsync(object key, CancellationToken cancellationToken);

    Task<object> AddAsync(object entity, CancellationToken cancellationToken);

    Task<object> UpdateAsync(object key, object entity, CancellationToken cancellationToken);

    Task DeleteAsync(object entity, CancellationToken cancellationToken);
}

public interface IRepository<TEntity, TPrimaryKey>
where TEntity : class
{
    Task<TEntity?> ReadAsync(TPrimaryKey key, CancellationToken cancellationToken);

    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken);

    Task<TEntity> UpdateAsync(TPrimaryKey key, TEntity entity, CancellationToken cancellationToken);

    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken);
}

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

public class Repository<TEntity, TPrimaryKey, TDbContext> : IRepository<TEntity, TPrimaryKey>
    where TEntity : class
    where TDbContext : DbContext
{
    readonly IDbContextFactory<TDbContext> dbContextFactory;

    public Repository(IDbContextFactory<TDbContext> dbContextFactory)
    {
        this.dbContextFactory = dbContextFactory;
    }

    public async Task<TEntity> ReadAsync(TPrimaryKey key, CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var result = await dbContext.FindAsync<TEntity>(key);
        
        return result;
    }

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var result = await dbContext.AddAsync(entity, cancellationToken);

        return result.Entity;
    }

    public async Task<TEntity> UpdateAsync(TPrimaryKey key, TEntity entity, CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var result = dbContext.Update(entity);

        return result.Entity;
    }

    public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var result = dbContext.Remove(entity);
    }
}