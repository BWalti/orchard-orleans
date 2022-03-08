namespace Orleans.Persistence.EntityFramework;

public interface IRepositoryCore
{
    Task<IProvideETag?> ReadAsync(object key, CancellationToken cancellationToken);

    Task<IProvideETag> AddAsync(IProvideETag entity, CancellationToken cancellationToken);

    Task<IProvideETag> UpdateAsync(object key, IProvideETag entity, CancellationToken cancellationToken);

    Task DeleteAsync(IProvideETag entity, CancellationToken cancellationToken);
}