namespace Orleans.Persistence.EntityFramework;

public interface IHasStringId
{
    string? Id { get; set; }
}