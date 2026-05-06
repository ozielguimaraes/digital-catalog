using FluentAssertions;
using MeuCatalogo.Application.Entities;
using Xunit;

namespace MeuCatalogo.Application.Tests.Entities;

public class ProdutoEntityTests
{
    [Fact]
    public void ObterPrecoUnitario_RetornaPreco_QuandoSemDesconto()
    {
        var produto = new Produto("p", Guid.NewGuid(), Guid.NewGuid(), preco: 100m, precoComDesconto: null, "info");

        produto.ObterPrecoUnitario().Should().Be(100m);
    }

    [Fact]
    public void ObterPrecoUnitario_RetornaPrecoComDesconto_QuandoDefinido()
    {
        var produto = new Produto("p", Guid.NewGuid(), Guid.NewGuid(), preco: 100m, precoComDesconto: 75m, "info");

        produto.ObterPrecoUnitario().Should().Be(75m);
    }

    [Fact]
    public void ObterPrecoUnitario_RetornaPrecoComDescontoMesmoSeMaior()
    {
        var produto = new Produto("p", Guid.NewGuid(), Guid.NewGuid(), preco: 50m, precoComDesconto: 60m, "info");

        produto.ObterPrecoUnitario().Should().Be(60m);
    }
}
