using EnsyInc.Loom.DataAccess.Models;

using EnsyNet.DataAccess.EntityFramework.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnsyInc.Loom.DataAccess.Configuration;

internal static class UserConfig
{
    public static void Configure(this EntityTypeBuilder<UserEntity> builder)
    {
        builder.ConfigureBaseProperties();

        builder.Property(e => e.EntraObjectId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(320);

        builder.HasIndex(e => e.EntraObjectId)
            .IsUnique()
            .HasFilter("[DeletedAt] IS NULL");
    }
}
