using EnsyInc.Loom.DataAccess.Configuration;
using EnsyInc.Loom.DataAccess.Models;

using Microsoft.EntityFrameworkCore;

namespace EnsyInc.Loom.DataAccess;

public sealed class LoomDbContext : DbContext
{
    public DbSet<ProjectEntity> Projects { get; init; }

    public LoomDbContext(DbContextOptions<LoomDbContext> options) : base(options)
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProjectEntity>().Configure();
    }
}
