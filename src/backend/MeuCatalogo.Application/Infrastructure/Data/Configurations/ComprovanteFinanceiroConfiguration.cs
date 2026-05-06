using MeuCatalogo.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeuCatalogo.Application.Infrastructure.Data.Configurations;

public class ComprovanteFinanceiroConfiguration : IEntityTypeConfiguration<ComprovanteFinanceiro>
{
    public void Configure(EntityTypeBuilder<ComprovanteFinanceiro> builder)
    {
        builder.ToTable("comprovantes_financeiros");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired().HasMaxLength(36);
        builder.Property(x => x.Descricao).HasColumnName("descricao").HasMaxLength(200);
        builder.Property(x => x.BasePath).HasColumnName("base_path").IsRequired().HasMaxLength(300);
        builder.Property(x => x.ContentType).HasColumnName("content_type").IsRequired().HasMaxLength(80);
        builder.Property(x => x.Size).HasColumnName("size").IsRequired();
        builder.Property(x => x.FileName).HasColumnName("file_name").IsRequired().HasMaxLength(200);
        builder.Property(x => x.DataCriacao).HasColumnName("data_criacao").IsRequired();
        builder.Property(x => x.DataAtualizacao).HasColumnName("data_atualizacao");
        builder.Property(x => x.Ativo).HasColumnName("ativo").IsRequired();

        builder.HasIndex(x => x.UserId);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
