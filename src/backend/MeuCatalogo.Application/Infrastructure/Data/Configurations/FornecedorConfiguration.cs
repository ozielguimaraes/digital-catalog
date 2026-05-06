using MeuCatalogo.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeuCatalogo.Application.Infrastructure.Data.Configurations;

public class FornecedorConfiguration : IEntityTypeConfiguration<Fornecedor>
{
    public void Configure(EntityTypeBuilder<Fornecedor> builder)
    {
        builder.ToTable("Fornecedores");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Nome).IsRequired().HasMaxLength(120);
        builder.Property(x => x.Categoria).HasMaxLength(80);
        builder.Property(x => x.NomeContato).HasMaxLength(120);
        builder.Property(x => x.Email).HasMaxLength(120);
        builder.Property(x => x.Telefone).HasMaxLength(30);
        builder.Property(x => x.Documento).HasMaxLength(30);
        builder.Property(x => x.Observacoes).HasMaxLength(500);
        builder.Property(x => x.DataCriacao).IsRequired();
        builder.Property(x => x.DataAtualizacao);
        builder.Property(x => x.UserId).IsRequired().HasMaxLength(36);

        builder.HasIndex(x => x.UserId);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
