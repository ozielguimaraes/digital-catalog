namespace MeuCatalogo.Application.DTOs.Responses;

public class FaturaResponse
{
    public Guid Id { get; set; }
    public Guid ContaId { get; set; }
    public string ContaNome { get; set; } = string.Empty;
    public int Mes { get; set; }
    public int Ano { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public DateTime DataVencimento { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal? ValorPago { get; set; }
    public decimal ValorEmAberto { get; set; }
    public bool Quitada { get; set; }
    public List<LancamentoResponse> Lancamentos { get; set; } = new();
}
