namespace Orleans.Persistence.EntityFramework;

public interface IRepositoryCore
{
    Task<object> ReadAsync(object key, CancellationToken cancellationToken);

    Task<object> AddAsync(object entity, CancellationToken cancellationToken);

    Task<object> UpdateAsync(object key, object entity, CancellationToken cancellationToken);

    Task DeleteAsync(object entity, CancellationToken cancellationToken);
}