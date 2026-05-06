using FluentAssertions;
using MeuCatalogo.Application.Services.Faturas;
using Xunit;

namespace MeuCatalogo.Application.Tests.Services.Faturas;

public class FaturaCalculatorTests
{
    [Fact]
    public void Calcular_FechamentoDia15_VencimentoDia25_RetornaDatasCorretas()
    {
        var (inicio, fim, venc) = FaturaCalculator.Calcular(diaFechamento: 15, diaVencimento: 25, mes: 5, ano: 2026);

        inicio.Should().Be(new DateTime(2026, 4, 16, 0, 0, 0, DateTimeKind.Utc));
        fim.Should().Be(new DateTime(2026, 5, 15, 0, 0, 0, DateTimeKind.Utc));
        venc.Should().Be(new DateTime(2026, 5, 25, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void Calcular_VencimentoAntesDoFechamento_AvancaMes()
    {
        var (_, fim, venc) = FaturaCalculator.Calcular(diaFechamento: 25, diaVencimento: 5, mes: 5, ano: 2026);

        fim.Should().Be(new DateTime(2026, 5, 25, 0, 0, 0, DateTimeKind.Utc));
        venc.Should().Be(new DateTime(2026, 6, 5, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void Calcular_FechamentoDia31_FevereiroNaoBissexto_AjustaParaUltimoDia()
    {
        var (inicio, fim, _) = FaturaCalculator.Calcular(diaFechamento: 31, diaVencimento: 10, mes: 3, ano: 2026);

        inicio.Should().Be(new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc));
        fim.Should().Be(new DateTime(2026, 3, 31, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void Calcular_FechamentoDia31_FevereiroBissexto_AjustaPara29()
    {
        var (_, fim, _) = FaturaCalculator.Calcular(diaFechamento: 31, diaVencimento: 10, mes: 2, ano: 2024);

        fim.Should().Be(new DateTime(2024, 2, 29, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void Calcular_VencimentoDia31_FevereiroNaoBissexto_AjustaPara28()
    {
        var (_, _, venc) = FaturaCalculator.Calcular(diaFechamento: 1, diaVencimento: 31, mes: 2, ano: 2026);

        venc.Should().Be(new DateTime(2026, 2, 28, 0, 0, 0, DateTimeKind.Utc));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(32)]
    public void Calcular_DiaFechamentoForaIntervalo_LancaExcecao(byte dia)
    {
        var act = () => FaturaCalculator.Calcular(diaFechamento: dia, diaVencimento: 10, mes: 5, ano: 2026);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(32)]
    public void Calcular_DiaVencimentoForaIntervalo_LancaExcecao(byte dia)
    {
        var act = () => FaturaCalculator.Calcular(diaFechamento: 15, diaVencimento: dia, mes: 5, ano: 2026);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void Calcular_MesForaIntervalo_LancaExcecao(int mes)
    {
        var act = () => FaturaCalculator.Calcular(diaFechamento: 15, diaVencimento: 10, mes: mes, ano: 2026);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(12, 2026, 1, 2027)]
    [InlineData(1, 2026, 2, 2026)]
    [InlineData(11, 2026, 12, 2026)]
    public void ProximoMes_Avanca(int mes, int ano, int mesEsperado, int anoEsperado)
    {
        var (m, a) = FaturaCalculator.ProximoMes(mes, ano);
        m.Should().Be(mesEsperado);
        a.Should().Be(anoEsperado);
    }

    [Fact]
    public void FaturaParaData_DataAntesFechamento_RetornaMesmoMes()
    {
        var (mes, ano) = FaturaCalculator.FaturaParaData(diaFechamento: 15, data: new DateTime(2026, 5, 10));
        mes.Should().Be(5);
        ano.Should().Be(2026);
    }

    [Fact]
    public void FaturaParaData_DataDepoisFechamento_RetornaProximoMes()
    {
        var (mes, ano) = FaturaCalculator.FaturaParaData(diaFechamento: 15, data: new DateTime(2026, 5, 20));
        mes.Should().Be(6);
        ano.Should().Be(2026);
    }

    [Fact]
    public void FaturaParaData_DezembroDepoisFechamento_AvancaParaJaneiroProximoAno()
    {
        var (mes, ano) = FaturaCalculator.FaturaParaData(diaFechamento: 15, data: new DateTime(2026, 12, 28));
        mes.Should().Be(1);
        ano.Should().Be(2027);
    }
}
