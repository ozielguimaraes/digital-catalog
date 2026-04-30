using MeuCatalogo.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeuCatalogo.Application.Infrastructure.Data.Configurations;

public class RecorrenciaConfiguration : IEntityTypeConfiguration<Recorrencia>
{
    public void Configure(EntityTypeBuilder<Recorrencia> builder)
    {
        builder.ToTable("recorrencias");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired().HasMaxLength(36);
        builder.Property(x => x.Tipo).HasColumnName("tipo").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Descricao).HasColumnName("descricao").IsRequired().HasMaxLength(150);
        builder.Property(x => x.ContaId).HasColumnName("conta_id").IsRequired();
        builder.Property(x => x.CategoriaFinanceiraId).HasColumnName("categoria_financeira_id").IsRequired();
        builder.Property(x => x.SubcategoriaFinanceiraId).HasColumnName("subcategoria_financeira_id");
        builder.Property(x => x.ValorPadrao).HasColumnName("valor_padrao").HasColumnType("decimal(18,2)");
        builder.Property(x => x.Periodicidade).HasColumnName("periodicidade").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.DiaDoMes).HasColumnName("dia_do_mes");
        builder.Property(x => x.DiaDaSemana).HasColumnName("dia_da_semana").HasConversion<int?>();
        builder.Property(x => x.DataInicio).HasColumnName("data_inicio").IsRequired();
        builder.Property(x => x.DataFim).HasColumnName("data_fim");
        builder.Property(x => x.ProximaData).HasColumnName("proxima_data").IsRequired();
        builder.Property(x => x.DataCriacao).HasColumnName("data_criacao").IsRequired();
        builder.Property(x => x.DataAtualizacao).HasColumnName("data_atualizacao");
        builder.Property(x => x.Ativo).HasColumnName("ativo").IsRequired();

        builder.HasIndex(x => new { x.UserId, x.Ativo });
        builder.HasIndex(x => x.ProximaData);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

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
    }
}
