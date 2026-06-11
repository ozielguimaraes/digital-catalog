using MeuCatalogo.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeuCatalogo.Application.Infrastructure.Data.Configurations;

public class ProdutoVariacaoConfiguration : IEntityTypeConfiguration<ProdutoVariacao>
{
    public void Configure(EntityTypeBuilder<ProdutoVariacao> builder)
    {
        builder.ToTable("ProdutoVariacoes");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.ProdutoId).IsRequired();
        builder.Property(x => x.Cor).IsRequired(false).HasMaxLength(50);
        builder.Property(x => x.Tamanho).IsRequired(false).HasMaxLength(50);
        builder.Property(x => x.Preco).HasPrecision(18, 2);
        builder.Property(x => x.Quantidade).IsRequired();
        builder.Property(x => x.DataCriacao).IsRequired();
        builder.Property(x => x.DataAtualizacao);

        // Relationships
        builder.HasOne(v => v.Produto)
            .WithMany(p => p.ProdutoVariacoes)
            .HasForeignKey(v => v.ProdutoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
