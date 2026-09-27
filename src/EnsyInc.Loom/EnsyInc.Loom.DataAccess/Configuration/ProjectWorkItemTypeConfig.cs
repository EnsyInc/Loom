using EnsyInc.Loom.DataAccess.Models;

using EnsyNet.DataAccess.EntityFramework.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnsyInc.Loom.DataAccess.Configuration;

internal static class ProjectWorkItemTypeConfig
{
    public static void Configure(this EntityTypeBuilder<ProjectWorkItemTypeEntity> builder)
    {
        builder.ConfigureBaseProperties();

        builder.HasOne<ProjectEntity>()
            .WithMany()
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<TplWorkItemEntity>()
            .WithMany()
            .HasForeignKey(e => e.TypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.ProjectId, e.TypeId })
            .IsUnique()
            .HasFilter("[DeletedAt] IS NULL");
    }
}
