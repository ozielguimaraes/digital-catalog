using MeuCatalogo.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeuCatalogo.Application.Infrastructure.Data.Configurations;

public class ItemPedidoConfiguration : IEntityTypeConfiguration<ItemPedido>
{
    public void Configure(EntityTypeBuilder<ItemPedido> builder)
    {
        builder.ToTable("ItensPedido");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.PedidoId).IsRequired();
        builder.Property(x => x.ProdutoId).IsRequired();
        builder.Property(x => x.Quantidade).IsRequired();
        builder.Property(x => x.PrecoUnitario).IsRequired().HasPrecision(18, 2);
        builder.Property(x => x.Subtotal).IsRequired().HasPrecision(18, 2);
        builder.Property(x => x.DataCriacao).IsRequired();
        builder.Property(x => x.DataAtualizacao);

        // Relationships
        builder.HasOne(i => i.Pedido)
            .WithMany(p => p.Itens)
            .HasForeignKey(i => i.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Produto)
            .WithMany(p => p.ItensPedido)
            .HasForeignKey(i => i.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Variacao)
            .WithMany()
            .HasForeignKey(i => i.VariacaoId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
