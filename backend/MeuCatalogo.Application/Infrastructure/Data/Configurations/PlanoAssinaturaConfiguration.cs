using MeuCatalogo.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeuCatalogo.Application.Infrastructure.Data.Configurations;

public class PlanoAssinaturaConfiguration : IEntityTypeConfiguration<PlanoAssinatura>
{
    public void Configure(EntityTypeBuilder<PlanoAssinatura> builder)
    {
        builder.ToTable("PlanosAssinatura");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Nome).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Descricao).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Preco).IsRequired().HasPrecision(18, 2);
        builder.Property(x => x.DuracaoEmMeses).IsRequired();
        builder.Property(x => x.LimiteProdutos).IsRequired();
        builder.Property(x => x.LimiteCatalogos).IsRequired();
        builder.Property(x => x.PermiteVariacoes).IsRequired();
        builder.Property(x => x.PermiteEstoque).IsRequired();
        builder.Property(x => x.PermiteRelatorios).IsRequired();
        builder.Property(x => x.PermiteExportacao).IsRequired();
        builder.Property(x => x.PermiteImportacao).IsRequired();
        builder.Property(x => x.PermitePersonalizacao).IsRequired();
        builder.Property(x => x.EhGratuito).IsRequired();
        builder.Property(x => x.DataCriacao).IsRequired();
        builder.Property(x => x.DataAtualizacao);

        // Relationships
        builder.HasMany(p => p.AssinaturasUsuarios)
            .WithOne(a => a.PlanoAssinatura)
            .HasForeignKey(a => a.PlanoAssinaturaId);
    }
}
