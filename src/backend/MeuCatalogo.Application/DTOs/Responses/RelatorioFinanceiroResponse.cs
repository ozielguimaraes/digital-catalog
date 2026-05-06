using MeuCatalogo.Application.Entities;

namespace MeuCatalogo.Application.DTOs.Responses;

public class RelatorioFinanceiroResponse
{
    public int Ano { get; set; }
    public int Mes { get; set; }
    public int Quantidade { get; set; }
    public List<RelatorioMesItem> Meses { get; set; } = new();
    public List<RelatorioCategoriaItem> Receitas { get; set; } = new();
    public List<RelatorioCategoriaItem> Despesas { get; set; } = new();
    public decimal TotalReceitas { get; set; }
    public decimal TotalDespesas { get; set; }
    public decimal Saldo { get; set; }
}

public class RelatorioMesItem
{
    public int Ano { get; set; }
    public int Mes { get; set; }
    public string Label { get; set; } = string.Empty;
    public decimal Receitas { get; set; }
    public decimal Despesas { get; set; }
    public decimal Saldo { get; set; }
}

public class RelatorioCategoriaItem
{
    public Guid? CategoriaFinanceiraId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Icone { get; set; }
    public string? Cor { get; set; }
    public CategoriaFinanceiraTipo Tipo { get; set; }
    public decimal Total { get; set; }
    public List<RelatorioMesItem> Mensal { get; set; } = new();
}
