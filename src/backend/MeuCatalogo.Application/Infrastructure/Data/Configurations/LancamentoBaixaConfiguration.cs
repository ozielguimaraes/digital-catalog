using MeuCatalogo.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeuCatalogo.Application.Infrastructure.Data.Configurations;

public class LancamentoBaixaConfiguration : IEntityTypeConfiguration<LancamentoBaixa>
{
    public void Configure(EntityTypeBuilder<LancamentoBaixa> builder)
    {
        builder.ToTable("lancamentos_baixas");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.LancamentoId).HasColumnName("lancamento_id").IsRequired();
        builder.Property(x => x.Data).HasColumnName("data").IsRequired();
        builder.Property(x => x.Valor).HasColumnName("valor").HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.ContaId).HasColumnName("conta_id").IsRequired();
        builder.Property(x => x.ComprovanteFinanceiroId).HasColumnName("comprovante_financeiro_id");
        builder.Property(x => x.Observacoes).HasColumnName("observacoes").HasMaxLength(500);
        builder.Property(x => x.DataCriacao).HasColumnName("data_criacao").IsRequired();
        builder.Property(x => x.DataAtualizacao).HasColumnName("data_atualizacao");
        builder.Property(x => x.Ativo).HasColumnName("ativo").IsRequired();

        builder.HasIndex(x => x.LancamentoId);
        builder.HasIndex(x => x.Data);

        builder.HasOne(x => x.Lancamento)
            .WithMany(l => l.Baixas)
            .HasForeignKey(x => x.LancamentoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Conta)
            .WithMany()
            .HasForeignKey(x => x.ContaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ComprovanteFinanceiro)
            .WithMany()
            .HasForeignKey(x => x.ComprovanteFinanceiroId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
