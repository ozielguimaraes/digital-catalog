using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeuCatalogo.Application.Infrastructure.Data.Configurations.Identity;

public class IdentityUserClaimConfiguration : IEntityTypeConfiguration<IdentityUserClaim<string>>
{
    public void Configure(EntityTypeBuilder<IdentityUserClaim<string>> builder)
    {
        // Table name
        builder.ToTable("AspNetUserClaims");

        // Primary key
        builder.HasKey(uc => uc.Id);

        // Properties
        builder.Property(uc => uc.UserId).HasMaxLength(36);
    }
}