using MeuCatalogo.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeuCatalogo.Application.Infrastructure.Data.Configurations;

public class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("Produtos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Nome).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Preco).IsRequired().HasPrecision(18, 2);
        builder.Property(x => x.PrecoComDesconto).HasPrecision(18, 2);
        builder.Property(x => x.InformacoesAdicionais).HasMaxLength(1000);
        builder.Property(x => x.DataCriacao).IsRequired();
        builder.Property(x => x.DataAtualizacao);

        // Relationships
        builder.HasOne(p => p.Categoria)
            .WithMany(c => c.Produtos)
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Catalogo)
            .WithMany(c => c.Produtos)
            .HasForeignKey(p => p.CatalogoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Estoque)
            .WithOne(e => e.Produto)
            .HasForeignKey<Estoque>(e => e.ProdutoId);
    }
}
