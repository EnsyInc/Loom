using EnsyInc.Loom.DataAccess.Models;

using EnsyNet.DataAccess.EntityFramework.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnsyInc.Loom.DataAccess.Configuration;

internal static class TplWorkItemFieldOptionConfig
{
    public static void Configure(this EntityTypeBuilder<TplWorkItemFieldOptionEntity> builder)
    {
        builder.ConfigureBaseProperties();

        builder.Property(e => e.Value)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.Label)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasOne<TplWorkItemFieldEntity>()
            .WithMany()
            .HasForeignKey(e => e.FieldId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.FieldId, e.Value })
            .IsUnique()
            .HasFilter("[DeletedAt] IS NULL");
    }
}
