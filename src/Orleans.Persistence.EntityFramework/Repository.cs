using Microsoft.EntityFrameworkCore;

namespace Orleans.Persistence.EntityFramework;

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

        await dbContext.SaveChangesAsync(cancellationToken);

        return result.Entity;
    }

    public async Task<TEntity> UpdateAsync(TPrimaryKey key, TEntity entity, CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var result = dbContext.Update(entity);

        await dbContext.SaveChangesAsync(cancellationToken);
        return result.Entity;
    }

    public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var result = dbContext.Remove(entity);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}