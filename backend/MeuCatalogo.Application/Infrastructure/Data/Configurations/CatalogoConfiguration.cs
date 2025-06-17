using MeuCatalogo.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeuCatalogo.Application.Infrastructure.Data.Configurations;

public class CatalogoConfiguration : IEntityTypeConfiguration<Catalogo>
{
    public void Configure(EntityTypeBuilder<Catalogo> builder)
    {
        builder.ToTable("Catalogos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Nome).IsRequired().HasMaxLength(100);
        builder.Property(x => x.NomeCurto).IsRequired().HasMaxLength(30);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(100);
        builder.Property(x => x.NumeroWhatsapp).IsRequired().HasMaxLength(15);
        builder.Property(x => x.Descricao).HasMaxLength(500);
        builder.Property(x => x.UserId).IsRequired().HasMaxLength(36);
        builder.Property(x => x.DataCriacao).IsRequired();
        builder.Property(x => x.DataAtualizacao);

        // Relationships
        builder.HasOne(c => c.User)
            .WithMany(u => u.Catalogos)
            .HasForeignKey(c => c.UserId);

        builder.HasMany(c => c.Produtos)
            .WithOne(p => p.Catalogo)
            .HasForeignKey(p => p.CatalogoId);
    }
}
