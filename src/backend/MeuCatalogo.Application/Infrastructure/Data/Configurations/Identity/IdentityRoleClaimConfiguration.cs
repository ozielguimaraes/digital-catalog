using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeuCatalogo.Application.Infrastructure.Data.Configurations.Identity;

public class IdentityRoleClaimConfiguration : IEntityTypeConfiguration<IdentityRoleClaim<string>>
{
    public void Configure(EntityTypeBuilder<IdentityRoleClaim<string>> builder)
    {
        // Table name
        builder.ToTable("AspNetRoleClaims");

        // Primary key
        builder.HasKey(rc => rc.Id);

        // Properties
        builder.Property(rc => rc.RoleId).HasMaxLength(36);
    }
}