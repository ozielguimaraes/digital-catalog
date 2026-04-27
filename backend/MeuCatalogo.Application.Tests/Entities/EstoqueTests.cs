using FluentAssertions;
using MeuCatalogo.Application.Entities;
using Xunit;

namespace MeuCatalogo.Application.Tests.Entities;

public class EstoqueTests
{
    [Fact]
    public void Construtor_SemArgumentos_DefineDisponivelTrue()
    {
        var estoque = new Estoque();

        estoque.Disponivel.Should().BeTrue();
    }

    [Fact]
    public void EhIlimitado_RetornaTrue_QuandoQuantidadeNull()
    {
        var estoque = new Estoque { Quantidade = null };

        estoque.EhIlimitado().Should().BeTrue();
    }

    [Fact]
    public void EhIlimitado_RetornaFalse_QuandoQuantidadeZero()
    {
        var estoque = new Estoque { Quantidade = 0 };

        estoque.EhIlimitado().Should().BeFalse();
    }

    [Theory]
    [InlineData(10, 5, true)]
    [InlineData(10, 10, true)]
    [InlineData(10, 11, false)]
    [InlineData(0, 1, false)]
    public void TemEstoqueSuficiente_AvaliaCorretamente(int? disponivel, int solicitado, bool esperado)
    {
        var estoque = new Estoque { Quantidade = disponivel };

        estoque.TemEstoqueSuficiente(solicitado).Should().Be(esperado);
    }

    [Fact]
    public void TemEstoqueSuficiente_RetornaTrue_QuandoIlimitado()
    {
        var estoque = new Estoque { Quantidade = null };

        estoque.TemEstoqueSuficiente(int.MaxValue).Should().BeTrue();
    }

    [Fact]
    public void DecrementarEstoque_SubtraiQuantidade()
    {
        var estoque = new Estoque { Quantidade = 10 };

        estoque.DecrementarEstoque(3);

        estoque.Quantidade.Should().Be(7);
    }

    [Fact]
    public void DecrementarEstoque_LancaExcecao_QuandoInsuficiente()
    {
        var estoque = new Estoque { Quantidade = 2 };

        var act = () => estoque.DecrementarEstoque(5);

        act.Should().Throw<InvalidOperationException>().WithMessage("Estoque insuficiente");
    }

    [Fact]
    public void IncrementarEstoque_AumentaQuantidade()
    {
        var estoque = new Estoque { Quantidade = 5 };

        estoque.IncrementarEstoque(7);

        estoque.Quantidade.Should().Be(12);
    }
}
