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

        builder.HasIndex(x => new { x.UserId, x.Tipo, x.Status });
        builder.HasIndex(x => x.DataVencimento);

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
    }
}
