using MeuCatalogo.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeuCatalogo.Application.Infrastructure.Data.Configurations;

public class CategoriaFinanceiraConfiguration : IEntityTypeConfiguration<CategoriaFinanceira>
{
    public void Configure(EntityTypeBuilder<CategoriaFinanceira> builder)
    {
        builder.ToTable("categorias_financeiras");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired().HasMaxLength(36);
        builder.Property(x => x.Tipo).HasColumnName("tipo").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Nome).HasColumnName("nome").IsRequired().HasMaxLength(80);
        builder.Property(x => x.IconeNome).HasColumnName("icone_nome").IsRequired().HasMaxLength(40);
        builder.Property(x => x.Cor).HasColumnName("cor").IsRequired().HasMaxLength(9);
        builder.Property(x => x.Ordem).HasColumnName("ordem");
        builder.Property(x => x.DataCriacao).HasColumnName("data_criacao").IsRequired();
        builder.Property(x => x.DataAtualizacao).HasColumnName("data_atualizacao");
        builder.Property(x => x.Ativo).HasColumnName("ativo").IsRequired();

        builder.HasIndex(x => new { x.UserId, x.Tipo, x.Ativo });

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Subcategorias)
            .WithOne(s => s.CategoriaFinanceira)
            .HasForeignKey(s => s.CategoriaFinanceiraId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
