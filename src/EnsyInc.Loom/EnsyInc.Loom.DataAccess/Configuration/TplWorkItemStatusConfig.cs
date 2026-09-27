using EnsyInc.Loom.DataAccess.Models;

using EnsyNet.DataAccess.EntityFramework.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnsyInc.Loom.DataAccess.Configuration;

internal static class TplWorkItemStatusConfig
{
    public static void Configure(this EntityTypeBuilder<TplWorkItemStatusEntity> builder)
    {
        builder.ConfigureBaseProperties();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.Category)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.HasIndex(e => e.Name)
            .IsUnique()
            .HasFilter("[DeletedAt] IS NULL");
    }
}
