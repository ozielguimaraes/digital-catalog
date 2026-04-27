using FluentAssertions;
using MeuCatalogo.Application.Entities;
using Xunit;

namespace MeuCatalogo.Application.Tests.Entities;

public class PedidoTests
{
    [Fact]
    public void Construtor_ComClienteId_InicializaStatusNovoEPagamentoACombinar()
    {
        var clienteId = Guid.NewGuid();

        var pedido = new Pedido(clienteId);

        pedido.ClienteId.Should().Be(clienteId);
        pedido.Status.Should().Be(StatusPedido.Novo);
        pedido.FormaPagamento.Should().Be(FormaPagamento.ACombinar);
        pedido.Itens.Should().BeEmpty();
        pedido.ValorTotal.Should().Be(0m);
    }

    [Fact]
    public void CalcularValorTotal_SomaSubtotalDeTodosItens()
    {
        var pedido = new Pedido(Guid.NewGuid());
        pedido.Itens.Add(new ItemPedido(pedido.Id, Guid.NewGuid(), quantidade: 2, precoUnitario: 10m));
        pedido.Itens.Add(new ItemPedido(pedido.Id, Guid.NewGuid(), quantidade: 3, precoUnitario: 5m));
        pedido.Itens.Add(new ItemPedido(pedido.Id, Guid.NewGuid(), quantidade: 1, precoUnitario: 99.99m));

        pedido.CalcularValorTotal();

        pedido.ValorTotal.Should().Be(2 * 10m + 3 * 5m + 99.99m);
    }

    [Fact]
    public void CalcularValorTotal_SemItens_RetornaZero()
    {
        var pedido = new Pedido(Guid.NewGuid());

        pedido.CalcularValorTotal();

        pedido.ValorTotal.Should().Be(0m);
    }

    [Theory]
    [InlineData(StatusPedido.Confirmado)]
    [InlineData(StatusPedido.Pago)]
    [InlineData(StatusPedido.Entregue)]
    [InlineData(StatusPedido.Cancelado)]
    public void AtualizarStatus_AlteraStatus(StatusPedido novo)
    {
        var pedido = new Pedido(Guid.NewGuid());

        pedido.AtualizarStatus(novo);

        pedido.Status.Should().Be(novo);
    }

    [Fact]
    public void DefinirFormaPagamento_AlteraFormaPagamento()
    {
        var pedido = new Pedido(Guid.NewGuid());

        pedido.DefinirFormaPagamento(FormaPagamento.Pix);

        pedido.FormaPagamento.Should().Be(FormaPagamento.Pix);
    }
}
