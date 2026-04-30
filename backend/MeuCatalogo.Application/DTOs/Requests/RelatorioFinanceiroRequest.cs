using MeuCatalogo.Application.Entities;

namespace MeuCatalogo.Application.DTOs.Requests;

public enum RegimeContabil
{
    Competencia,
    Caixa
}

public class RelatorioFinanceiroRequest
{
    public int Ano { get; set; }
    public int Mes { get; set; }
    public int Quantidade { get; set; } = 1;
    public RegimeContabil Regime { get; set; } = RegimeContabil.Caixa;
    public List<Guid>? ContaIds { get; set; }
    public List<Guid>? CategoriaFinanceiraIds { get; set; }
    public CategoriaFinanceiraTipo? Tipo { get; set; }
    public bool ApenasRealizados { get; set; } = false;
}
