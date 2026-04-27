using FluentAssertions;
using MeuCatalogo.Application.Entities;
using Xunit;

namespace MeuCatalogo.Application.Tests.Entities;

public class AssinaturaUsuarioTests
{
    [Fact]
    public void EstaAtiva_RetornaTrue_QuandoVigenteSemCancelamento()
    {
        var assinatura = new AssinaturaUsuario("u", Guid.NewGuid(), DateTime.UtcNow.AddDays(10));

        assinatura.EstaAtiva().Should().BeTrue();
    }

    [Fact]
    public void EstaAtiva_RetornaFalse_QuandoDataFimExpirada()
    {
        var assinatura = new AssinaturaUsuario("u", Guid.NewGuid(), DateTime.UtcNow.AddDays(-1));

        assinatura.EstaAtiva().Should().BeFalse();
    }

    [Fact]
    public void EstaAtiva_RetornaFalse_QuandoCancelada()
    {
        var assinatura = new AssinaturaUsuario("u", Guid.NewGuid(), DateTime.UtcNow.AddDays(10));
        assinatura.Cancelar("teste");

        assinatura.EstaAtiva().Should().BeFalse();
    }

    [Fact]
    public void Cancelar_DefineDataCancelamentoEMotivo()
    {
        var assinatura = new AssinaturaUsuario("u", Guid.NewGuid(), DateTime.UtcNow.AddDays(10));

        assinatura.Cancelar("não usei");

        assinatura.Ativa.Should().BeFalse();
        assinatura.DataCancelamento.Should().NotBeNull();
        assinatura.MotivoCancelamento.Should().Be("não usei");
    }

    [Fact]
    public void Renovar_LimpaCancelamentoEAtualizaDataFim()
    {
        var assinatura = new AssinaturaUsuario("u", Guid.NewGuid(), DateTime.UtcNow.AddDays(10));
        assinatura.Cancelar("desisti");

        var novaData = DateTime.UtcNow.AddDays(30);
        assinatura.Renovar(novaData, transacaoId: "tx-123", valorPago: 99.9m);

        assinatura.Ativa.Should().BeTrue();
        assinatura.DataFim.Should().Be(novaData);
        assinatura.DataCancelamento.Should().BeNull();
        assinatura.MotivoCancelamento.Should().BeNull();
        assinatura.TransacaoId.Should().Be("tx-123");
        assinatura.ValorPago.Should().Be(99.9m);
    }
}
