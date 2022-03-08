namespace Silo;

using Grains;

using Microsoft.EntityFrameworkCore;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public DbSet<CounterState> CounterStates { get; set; }

    public DbSet<EnergyConsumption> EnergyConsumptions { get; set; }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //{
    //    base.OnConfiguring(optionsBuilder);
    //    optionsBuilder.UseNpgsql("Host=localhost;Database=demo;Username=demo;Password=secretPassword");
    //}
}