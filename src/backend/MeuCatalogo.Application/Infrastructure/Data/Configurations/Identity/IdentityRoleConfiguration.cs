using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeuCatalogo.Application.Infrastructure.Data.Configurations.Identity;

public class IdentityRoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        // Table name
        builder.ToTable("AspNetRoles");

        // Primary key
        builder.HasKey(r => r.Id);

        // Properties
        builder.Property(r => r.Id).HasMaxLength(36);
        builder.Property(r => r.Name).HasMaxLength(100);
        builder.Property(r => r.NormalizedName).HasMaxLength(100);

        // Indexes
        builder.HasIndex(r => r.NormalizedName).IsUnique();
    }
}