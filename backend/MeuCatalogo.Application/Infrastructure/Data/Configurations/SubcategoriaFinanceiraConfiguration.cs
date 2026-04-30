using MeuCatalogo.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeuCatalogo.Application.Infrastructure.Data.Configurations;

public class SubcategoriaFinanceiraConfiguration : IEntityTypeConfiguration<SubcategoriaFinanceira>
{
    public void Configure(EntityTypeBuilder<SubcategoriaFinanceira> builder)
    {
        builder.ToTable("subcategorias_financeiras");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.CategoriaFinanceiraId).HasColumnName("categoria_financeira_id").IsRequired();
        builder.Property(x => x.Nome).HasColumnName("nome").IsRequired().HasMaxLength(80);
        builder.Property(x => x.IconeNome).HasColumnName("icone_nome").HasMaxLength(40);
        builder.Property(x => x.Cor).HasColumnName("cor").HasMaxLength(9);
        builder.Property(x => x.Ordem).HasColumnName("ordem");
        builder.Property(x => x.DataCriacao).HasColumnName("data_criacao").IsRequired();
        builder.Property(x => x.DataAtualizacao).HasColumnName("data_atualizacao");
        builder.Property(x => x.Ativo).HasColumnName("ativo").IsRequired();

        builder.HasIndex(x => x.CategoriaFinanceiraId);
    }
}
