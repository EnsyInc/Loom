using EnsyInc.Loom.DataAccess.Configuration;
using EnsyInc.Loom.DataAccess.Models;

using Microsoft.EntityFrameworkCore;

namespace EnsyInc.Loom.DataAccess;

public sealed class LoomDbContext : DbContext
{
    public DbSet<ProjectEntity> Projects { get; init; }

    public DbSet<TplWorkItemStatusEntity> TplWorkItemStatuses { get; init; }

    public DbSet<TplWorkItemEntity> TplWorkItems { get; init; }

    public DbSet<TplWorkItemFieldEntity> TplWorkItemFields { get; init; }

    public DbSet<TplWorkItemFieldOptionEntity> TplWorkItemFieldOptions { get; init; }

    public DbSet<StatusTransitionEntity> StatusTransitions { get; init; }

    public DbSet<ProjectWorkItemTypeEntity> ProjectWorkItemTypes { get; init; }

    public DbSet<WorkItemTypeStatusEntity> WorkItemTypeStatuses { get; init; }

    public LoomDbContext(DbContextOptions<LoomDbContext> options) : base(options)
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProjectEntity>().Configure();
        modelBuilder.Entity<TplWorkItemStatusEntity>().Configure();
        modelBuilder.Entity<TplWorkItemEntity>().Configure();
        modelBuilder.Entity<TplWorkItemFieldEntity>().Configure();
        modelBuilder.Entity<TplWorkItemFieldOptionEntity>().Configure();
        modelBuilder.Entity<StatusTransitionEntity>().Configure();
        modelBuilder.Entity<ProjectWorkItemTypeEntity>().Configure();
        modelBuilder.Entity<WorkItemTypeStatusEntity>().Configure();
    }
}
