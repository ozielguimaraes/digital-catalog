using FluentAssertions;
using MeuCatalogo.Application.Entities;
using Xunit;

namespace MeuCatalogo.Application.Tests.Entities;

public class ItemPedidoTests
{
    [Fact]
    public void Construtor_CalculaSubtotalAutomaticamente()
    {
        var item = new ItemPedido(Guid.NewGuid(), Guid.NewGuid(), quantidade: 4, precoUnitario: 12.50m);

        item.Subtotal.Should().Be(50m);
    }

    [Theory]
    [InlineData(1, 10, 10)]
    [InlineData(0, 99.99, 0)]
    [InlineData(10, 1.5, 15)]
    [InlineData(3, 33.33, 99.99)]
    public void CalcularSubtotal_QuantidadeXPreco(int qtd, decimal preco, decimal esperado)
    {
        var item = new ItemPedido(Guid.NewGuid(), Guid.NewGuid(), qtd, preco);

        item.CalcularSubtotal();

        item.Subtotal.Should().Be(esperado);
    }

    [Fact]
    public void AtualizarQuantidade_RecalculaSubtotal()
    {
        var item = new ItemPedido(Guid.NewGuid(), Guid.NewGuid(), quantidade: 2, precoUnitario: 10m);

        item.AtualizarQuantidade(5);

        item.Quantidade.Should().Be(5);
        item.Subtotal.Should().Be(50m);
    }
}
