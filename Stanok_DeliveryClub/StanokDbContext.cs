using Microsoft.EntityFrameworkCore;
using Stanok.DataAccess.Configurations;
using Stanok.DataAccess.Entities;

namespace Stanok.DataAccess;

public class StanokDbContext(DbContextOptions<StanokDbContext> options) : DbContext(options)
{
    public DbSet<StanokEntity> Stanoks { get; set; }
    public DbSet<DeliveryEntity> Deliveries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new StanokConfiguration());
        modelBuilder.ApplyConfiguration(new DeliveryConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
