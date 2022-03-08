using System.ComponentModel.DataAnnotations;

namespace Orleans.Persistence.EntityFramework;

public class EfPersistableState<TPrimaryKey> : IProvideETag
{
    /// <summary>An e-tag that allows optimistic concurrency checks at the storage provider level.</summary>
    public int ETag { get; set; }

    [Required]
    public TPrimaryKey Id { get; set; }
}

public interface IProvideETag
{
    /// <summary>An e-tag that allows optimistic concurrency checks at the storage provider level.</summary>
    int ETag { get; set; }
}