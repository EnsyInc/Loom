using EnsyInc.Loom.DataAccess.Models;

using EnsyNet.DataAccess.EntityFramework.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnsyInc.Loom.DataAccess.Configuration;

internal static class WorkItemTypeStatusConfig
{
    public static void Configure(this EntityTypeBuilder<WorkItemTypeStatusEntity> builder)
    {
        builder.ConfigureBaseProperties();

        builder.HasOne<TplWorkItemEntity>()
            .WithMany()
            .HasForeignKey(e => e.TypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<TplWorkItemStatusEntity>()
            .WithMany()
            .HasForeignKey(e => e.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.TypeId, e.StatusId })
            .IsUnique()
            .HasFilter("[DeletedAt] IS NULL");
    }
}
