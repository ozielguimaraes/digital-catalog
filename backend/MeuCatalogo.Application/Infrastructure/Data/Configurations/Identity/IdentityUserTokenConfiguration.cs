using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeuCatalogo.Application.Infrastructure.Data.Configurations.Identity;

public class IdentityUserTokenConfiguration : IEntityTypeConfiguration<IdentityUserToken<string>>
{
    public void Configure(EntityTypeBuilder<IdentityUserToken<string>> builder)
    {
        // Table name
        builder.ToTable("AspNetUserTokens");

        // Primary key
        builder.HasKey(t => new { t.UserId, t.LoginProvider, t.Name });

        // Properties
        builder.Property(t => t.UserId).HasMaxLength(36);
        builder.Property(t => t.LoginProvider).HasMaxLength(128);
        builder.Property(t => t.Name).HasMaxLength(128);
    }
}