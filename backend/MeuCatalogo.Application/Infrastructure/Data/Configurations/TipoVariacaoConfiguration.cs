using MeuCatalogo.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeuCatalogo.Application.Infrastructure.Data.Configurations;

public class TipoVariacaoConfiguration : IEntityTypeConfiguration<TipoVariacao>
{
    public void Configure(EntityTypeBuilder<TipoVariacao> builder)
    {
        builder.ToTable("TipoVariacao");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Nome).IsRequired();
        builder.Property(x => x.DataCriacao).IsRequired();
        builder.Property(x => x.DataAtualizacao);
    }
}