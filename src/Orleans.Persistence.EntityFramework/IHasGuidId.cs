namespace Orleans.Persistence.EntityFramework;

public interface IHasGuidId
{
    Guid? Id { get; set; }
}