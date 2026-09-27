using EnsyInc.Loom.DataAccess.Models;

using EnsyNet.DataAccess.EntityFramework.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnsyInc.Loom.DataAccess.Configuration;

internal static class TplWorkItemFieldConfig
{
    public static void Configure(this EntityTypeBuilder<TplWorkItemFieldEntity> builder)
    {
        builder.ConfigureBaseProperties();

        builder.Property(e => e.Key)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(e => e.Label)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.DataType)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.HasOne<TplWorkItemEntity>()
            .WithMany()
            .HasForeignKey(e => e.TypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.TypeId, e.Key })
            .IsUnique()
            .HasFilter("[DeletedAt] IS NULL");
    }
}
