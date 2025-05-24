using MeuCatalogo.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeuCatalogo.Application.Infrastructure.Data.Configurations;

public class OpcaoVariacaoConfiguration : IEntityTypeConfiguration<OpcaoVariacao>
{
    public void Configure(EntityTypeBuilder<OpcaoVariacao> builder)
    {
        builder.ToTable("OpcaoVariacao");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Valor).IsRequired();
        builder.Property(x => x.TipoVariacaoId).IsRequired();
        builder.Property(x => x.DataCriacao).IsRequired();
        builder.Property(x => x.DataAtualizacao);

        // Relationship with TipoVariacao
        builder.HasOne(o => o.TipoVariacao)
            .WithMany()
            .HasForeignKey(o => o.TipoVariacaoId)
            .OnDelete(DeleteBehavior.NoAction); // Change from Cascade to NoAction
    }
}