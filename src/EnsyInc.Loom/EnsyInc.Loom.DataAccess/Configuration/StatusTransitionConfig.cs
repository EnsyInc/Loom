using EnsyInc.Loom.DataAccess.Models;

using EnsyNet.DataAccess.EntityFramework.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnsyInc.Loom.DataAccess.Configuration;

internal static class StatusTransitionConfig
{
    public static void Configure(this EntityTypeBuilder<StatusTransitionEntity> builder)
    {
        builder.ConfigureBaseProperties();

        builder.HasOne<TplWorkItemEntity>()
            .WithMany()
            .HasForeignKey(e => e.TypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<TplWorkItemStatusEntity>()
            .WithMany()
            .HasForeignKey(e => e.FromStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<TplWorkItemStatusEntity>()
            .WithMany()
            .HasForeignKey(e => e.ToStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.TypeId, e.FromStatusId, e.ToStatusId })
            .IsUnique()
            .HasFilter("[DeletedAt] IS NULL");

        builder.ToTable(t => t.HasCheckConstraint("CK_StatusTransition_FromToDifferent", "[FromStatusId] <> [ToStatusId]"));
    }
}
