namespace Orleans.Persistence.EntityFramework;

public interface IHasLongId
{
    long? Id { get; set; }
}