namespace MeuCatalogo.Application.DTOs.Responses;

public class ExtratoResponse
{
    public DateTime PeriodoInicio { get; set; }
    public DateTime PeriodoFim { get; set; }
    public List<Guid> ContaIds { get; set; } = new();
    public decimal SaldoInicial { get; set; }
    public decimal SaldoFinal { get; set; }
    public decimal TotalEntradas { get; set; }
    public decimal TotalSaidas { get; set; }
    public List<ExtratoMovimentoResponse> Movimentos { get; set; } = new();
    public List<ExtratoSaldoDiarioResponse> SaldosDiarios { get; set; } = new();
}

public enum ExtratoMovimentoTipo
{
    Entrada,
    Saida
}

public enum ExtratoMovimentoOrigem
{
    Lancamento,
    Baixa
}

public class ExtratoMovimentoResponse
{
    public Guid Id { get; set; }
    public ExtratoMovimentoOrigem Origem { get; set; }
    public Guid LancamentoId { get; set; }
    public Guid ContaId { get; set; }
    public string ContaNome { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public ExtratoMovimentoTipo Tipo { get; set; }
    public decimal Valor { get; set; }
    public decimal SaldoApos { get; set; }
    public string? CategoriaNome { get; set; }
    public string? CategoriaIcone { get; set; }
    public string? CategoriaCor { get; set; }
    public string? Observacoes { get; set; }
}

public class ExtratoSaldoDiarioResponse
{
    public DateTime Data { get; set; }
    public decimal Entradas { get; set; }
    public decimal Saidas { get; set; }
    public decimal SaldoFinalDia { get; set; }
}
