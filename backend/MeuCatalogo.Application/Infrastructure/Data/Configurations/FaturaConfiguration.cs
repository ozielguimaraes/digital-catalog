using MeuCatalogo.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeuCatalogo.Application.Infrastructure.Data.Configurations;

public class FaturaConfiguration : IEntityTypeConfiguration<Fatura>
{
    public void Configure(EntityTypeBuilder<Fatura> builder)
    {
        builder.ToTable("faturas");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.ContaId).HasColumnName("conta_id").IsRequired();
        builder.Property(x => x.Mes).HasColumnName("mes").IsRequired();
        builder.Property(x => x.Ano).HasColumnName("ano").IsRequired();
        builder.Property(x => x.DataInicio).HasColumnName("data_inicio").IsRequired();
        builder.Property(x => x.DataFim).HasColumnName("data_fim").IsRequired();
        builder.Property(x => x.DataVencimento).HasColumnName("data_vencimento").IsRequired();
        builder.Property(x => x.ValorPago).HasColumnName("valor_pago").HasColumnType("decimal(18,2)");
        builder.Property(x => x.DataCriacao).HasColumnName("data_criacao").IsRequired();
        builder.Property(x => x.DataAtualizacao).HasColumnName("data_atualizacao");
        builder.Property(x => x.Ativo).HasColumnName("ativo").IsRequired();

        builder.HasIndex(x => new { x.ContaId, x.Ano, x.Mes }).IsUnique();
        builder.HasIndex(x => x.DataVencimento);

        builder.HasOne(x => x.Conta)
            .WithMany()
            .HasForeignKey(x => x.ContaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
