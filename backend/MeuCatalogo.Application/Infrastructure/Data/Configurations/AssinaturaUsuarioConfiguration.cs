using MeuCatalogo.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeuCatalogo.Application.Infrastructure.Data.Configurations;

public class AssinaturaUsuarioConfiguration : IEntityTypeConfiguration<AssinaturaUsuario>
{
    public void Configure(EntityTypeBuilder<AssinaturaUsuario> builder)
    {
        builder.ToTable("AssinaturasUsuarios");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.UserId).IsRequired().HasMaxLength(36);
        builder.Property(x => x.PlanoAssinaturaId).IsRequired();
        builder.Property(x => x.DataInicio).IsRequired();
        builder.Property(x => x.DataFim).IsRequired();
        builder.Property(x => x.Ativa).IsRequired();
        builder.Property(x => x.TransacaoId).HasMaxLength(100);
        builder.Property(x => x.MetodoPagamento).HasMaxLength(50);
        builder.Property(x => x.ValorPago).HasPrecision(18, 2);
        builder.Property(x => x.RenovacaoAutomatica).IsRequired();
        builder.Property(x => x.MotivoCancelamento).HasMaxLength(500);
        builder.Property(x => x.DataCriacao).IsRequired();
        builder.Property(x => x.DataAtualizacao);

        // Relationships
        builder.HasOne(a => a.User)
            .WithMany(u => u.Assinaturas)
            .HasForeignKey(a => a.UserId);

        builder.HasOne(a => a.PlanoAssinatura)
            .WithMany(p => p.AssinaturasUsuarios)
            .HasForeignKey(a => a.PlanoAssinaturaId);
    }
}