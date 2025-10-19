using MeuCatalogo.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeuCatalogo.Application.Infrastructure.Data.Configurations;

public class ProdutoImagemConfiguration : IEntityTypeConfiguration<ProdutoImagem>
{
    public void Configure(EntityTypeBuilder<ProdutoImagem> builder)
    {
        builder.ToTable("ProdutoImagens");

        builder.HasKey(pi => pi.Id);

        builder.Property(pi => pi.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(pi => pi.Url)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(pi => pi.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(pi => pi.Size)
            .IsRequired();

        builder.Property(pi => pi.IsPrincipal)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(pi => pi.Ordem)
            .IsRequired()
            .HasDefaultValue(0);

        // Relacionamento com Produto
        builder.HasOne(pi => pi.Produto)
            .WithMany(p => p.Imagens)
            .HasForeignKey(pi => pi.ProdutoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(pi => pi.ProdutoId);
        builder.HasIndex(pi => pi.IsPrincipal);
        builder.HasIndex(pi => pi.Ordem);
    }
}
