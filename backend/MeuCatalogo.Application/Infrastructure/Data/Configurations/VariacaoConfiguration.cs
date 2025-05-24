using MeuCatalogo.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeuCatalogo.Application.Infrastructure.Data.Configurations;

public class VariacaoConfiguration : IEntityTypeConfiguration<Variacao>
{
    public void Configure(EntityTypeBuilder<Variacao> builder)
    {
        builder.ToTable("Variacoes");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.ProdutoId).IsRequired();
        builder.Property(x => x.TipoVariacaoId).IsRequired();
        builder.Property(x => x.OpcaoVariacaoId).IsRequired();
        builder.Property(x => x.DataCriacao).IsRequired();
        builder.Property(x => x.DataAtualizacao);

        // Relationships
        builder.HasOne(v => v.Produto)
            .WithMany(p => p.Variacoes)
            .HasForeignKey(v => v.ProdutoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Fix for the circular reference issue
        builder.HasOne(v => v.TipoVariacao)
            .WithMany()
            .HasForeignKey(v => v.TipoVariacaoId)
            .OnDelete(DeleteBehavior.NoAction); // Change from Cascade to NoAction

        builder.HasOne(v => v.OpcaoVariacao)
            .WithMany()
            .HasForeignKey(v => v.OpcaoVariacaoId)
            .OnDelete(DeleteBehavior.NoAction); // Change from Cascade to NoAction
    }
}