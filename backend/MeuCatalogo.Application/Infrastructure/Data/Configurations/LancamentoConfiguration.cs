using MeuCatalogo.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeuCatalogo.Application.Infrastructure.Data.Configurations;

public class LancamentoConfiguration : IEntityTypeConfiguration<Lancamento>
{
    public void Configure(EntityTypeBuilder<Lancamento> builder)
    {
        builder.ToTable("Lancamentos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Descricao).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Valor).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.DataVencimento).IsRequired();
        builder.Property(x => x.DataPagamento);
        builder.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Observacoes).HasMaxLength(500);
        builder.Property(x => x.UserId).IsRequired().HasMaxLength(36);
        builder.Property(x => x.DataCriacao).IsRequired();

        builder.Property(x => x.ContaId).HasColumnName("conta_id");
        builder.Property(x => x.CategoriaFinanceiraId).HasColumnName("categoria_financeira_id");
        builder.Property(x => x.SubcategoriaFinanceiraId).HasColumnName("subcategoria_financeira_id");
        builder.Property(x => x.Operacao).HasColumnName("operacao").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.TipoTransferencia).HasColumnName("tipo_transferencia").HasConversion<string?>().HasMaxLength(20);
        builder.Property(x => x.LancamentoTransferenciaId).HasColumnName("lancamento_transferencia_id");
        builder.Property(x => x.ParcelaAtual).HasColumnName("parcela_atual");
        builder.Property(x => x.ParcelaTotal).HasColumnName("parcela_total");
        builder.Property(x => x.FaturaId).HasColumnName("fatura_id");
        builder.Property(x => x.RecorrenciaId).HasColumnName("recorrencia_id");
        builder.Property(x => x.ComprovanteFinanceiroId).HasColumnName("comprovante_financeiro_id");
        builder.Property(x => x.Realizado).HasColumnName("realizado").IsRequired();

        builder.HasIndex(x => new { x.UserId, x.Tipo, x.Status });
        builder.HasIndex(x => x.DataVencimento);
        builder.HasIndex(x => new { x.UserId, x.ContaId });
        builder.HasIndex(x => new { x.UserId, x.CategoriaFinanceiraId });
        builder.HasIndex(x => x.RecorrenciaId);
        builder.HasIndex(x => x.FaturaId);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Pedido)
            .WithMany()
            .HasForeignKey(x => x.PedidoId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Fornecedor)
            .WithMany()
            .HasForeignKey(x => x.FornecedorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Conta)
            .WithMany()
            .HasForeignKey(x => x.ContaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CategoriaFinanceira)
            .WithMany()
            .HasForeignKey(x => x.CategoriaFinanceiraId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SubcategoriaFinanceira)
            .WithMany()
            .HasForeignKey(x => x.SubcategoriaFinanceiraId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.LancamentoTransferencia)
            .WithMany()
            .HasForeignKey(x => x.LancamentoTransferenciaId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Fatura)
            .WithMany(f => f.Lancamentos)
            .HasForeignKey(x => x.FaturaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Recorrencia)
            .WithMany()
            .HasForeignKey(x => x.RecorrenciaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.ComprovanteFinanceiro)
            .WithMany()
            .HasForeignKey(x => x.ComprovanteFinanceiroId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
