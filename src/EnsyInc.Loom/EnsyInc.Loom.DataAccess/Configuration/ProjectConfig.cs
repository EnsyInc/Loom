using EnsyInc.Loom.DataAccess.Models;

using EnsyNet.DataAccess.EntityFramework.Configuration;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnsyInc.Loom.DataAccess.Configuration;

internal static class ProjectConfig
{
    public static void Configure(this EntityTypeBuilder<ProjectEntity> builder)
    {
        builder.ConfigureBaseProperties();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(256);
    }
}
