using MeuCatalogo.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeuCatalogo.Application.Infrastructure.Data.Configurations;

public class ContaConfiguration : IEntityTypeConfiguration<Conta>
{
    public void Configure(EntityTypeBuilder<Conta> builder)
    {
        builder.ToTable("contas");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired().HasMaxLength(36);
        builder.Property(x => x.Nome).HasColumnName("nome").IsRequired().HasMaxLength(80);
        builder.Property(x => x.Tipo).HasColumnName("tipo").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Cor).HasColumnName("cor").IsRequired().HasMaxLength(9);
        builder.Property(x => x.Ordem).HasColumnName("ordem");
        builder.Property(x => x.Limite).HasColumnName("limite").HasColumnType("decimal(18,2)");
        builder.Property(x => x.DiaFechamento).HasColumnName("dia_fechamento");
        builder.Property(x => x.DiaVencimento).HasColumnName("dia_vencimento");
        builder.Property(x => x.SaldoInicial).HasColumnName("saldo_inicial").HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.DataCriacao).HasColumnName("data_criacao").IsRequired();
        builder.Property(x => x.DataAtualizacao).HasColumnName("data_atualizacao");
        builder.Property(x => x.Ativo).HasColumnName("ativo").IsRequired();

        builder.HasIndex(x => new { x.UserId, x.Ativo });

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
