using EnsyInc.Loom.DataAccess.Models;

using EnsyNet.DataAccess.EntityFramework.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnsyInc.Loom.DataAccess.Configuration;

internal static class TplWorkItemConfig
{
    public static void Configure(this EntityTypeBuilder<TplWorkItemEntity> builder)
    {
        builder.ConfigureBaseProperties();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.IconUrl)
            .HasMaxLength(2048);

        builder.HasOne<TplWorkItemStatusEntity>()
            .WithMany()
            .HasForeignKey(e => e.InitialStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.Name)
            .IsUnique()
            .HasFilter("[DeletedAt] IS NULL");
    }
}
