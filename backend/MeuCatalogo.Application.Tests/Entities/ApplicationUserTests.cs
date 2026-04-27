using FluentAssertions;
using MeuCatalogo.Application.Entities;
using Xunit;

namespace MeuCatalogo.Application.Tests.Entities;

public class ApplicationUserTests
{
    private static PlanoAssinatura NovoPlano(int limiteProdutos, int limiteCatalogos) =>
        new("Plano", "desc", 0m, 1, limiteProdutos, limiteCatalogos, true, true, true, true, true, true, true);

    private static AssinaturaUsuario NovaAssinaturaAtiva(PlanoAssinatura plano) =>
        new("user-1", plano.Id, DateTime.UtcNow.AddDays(30)) { PlanoAssinatura = plano };

    [Fact]
    public void TemPlanoAtivo_RetornaFalse_QuandoSemAssinatura()
    {
        var user = new ApplicationUser();

        user.TemPlanoAtivo().Should().BeFalse();
    }

    [Fact]
    public void TemPlanoAtivo_RetornaTrue_QuandoTemAssinaturaVigente()
    {
        var plano = NovoPlano(10, 1);
        var user = new ApplicationUser();
        user.Assinaturas.Add(NovaAssinaturaAtiva(plano));

        user.TemPlanoAtivo().Should().BeTrue();
    }

    [Fact]
    public void ObterAssinaturaAtiva_IgnoraExpiradas()
    {
        var plano = NovoPlano(10, 1);
        var expirada = new AssinaturaUsuario("user-1", plano.Id, DateTime.UtcNow.AddDays(-5)) { PlanoAssinatura = plano };
        var ativa = NovaAssinaturaAtiva(plano);

        var user = new ApplicationUser();
        user.Assinaturas.Add(expirada);
        user.Assinaturas.Add(ativa);

        user.ObterAssinaturaAtiva().Should().Be(ativa);
    }

    [Fact]
    public void PodeAdicionarProduto_RetornaTrue_QuandoSemPlano()
    {
        var user = new ApplicationUser();

        user.PodeAdicionarProduto(quantidadeAtual: 999).Should().BeTrue();
    }

    [Theory]
    [InlineData(0, 999, true)]   // limite <= 0 = ilimitado
    [InlineData(-1, 999, true)]  // limite negativo também ilimitado
    [InlineData(10, 9, true)]
    [InlineData(10, 10, false)]  // atingiu limite
    [InlineData(10, 11, false)]
    public void PodeAdicionarProduto_RespeitaLimiteDoPlano(int limite, int atual, bool esperado)
    {
        var plano = NovoPlano(limiteProdutos: limite, limiteCatalogos: 0);
        var user = new ApplicationUser();
        user.Assinaturas.Add(NovaAssinaturaAtiva(plano));

        user.PodeAdicionarProduto(atual).Should().Be(esperado);
    }

    [Theory]
    [InlineData(0, 999, true)]
    [InlineData(3, 2, true)]
    [InlineData(3, 3, false)]
    public void PodeAdicionarCatalogo_RespeitaLimiteDoPlano(int limite, int atual, bool esperado)
    {
        var plano = NovoPlano(limiteProdutos: 0, limiteCatalogos: limite);
        var user = new ApplicationUser();
        user.Assinaturas.Add(NovaAssinaturaAtiva(plano));

        user.PodeAdicionarCatalogo(atual).Should().Be(esperado);
    }
}
