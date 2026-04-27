using MeuCatalogo.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeuCatalogo.Application.Infrastructure.Data.Configurations;

public class EstoqueConfiguration : IEntityTypeConfiguration<Estoque>
{
    public void Configure(EntityTypeBuilder<Estoque> builder)
    {
        builder.ToTable("Estoques");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.ProdutoId).IsRequired();
        builder.Property(x => x.Quantidade).HasPrecision(18, 2);
        builder.Property(x => x.QuantidadeMinima).HasPrecision(18, 2);
        builder.Property(x => x.QuantidadeMaxima).HasPrecision(18, 2);
        builder.Property(x => x.Disponivel).IsRequired();
        builder.Property(x => x.DataCriacao).IsRequired();
        builder.Property(x => x.DataAtualizacao);

        // Relationships
        builder.HasOne(e => e.Produto)
            .WithOne(p => p.Estoque)
            .HasForeignKey<Estoque>(e => e.ProdutoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
