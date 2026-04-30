namespace MeuCatalogo.Application.DTOs.Responses;

public class LancamentoBaixaResponse
{
    public Guid Id { get; set; }
    public Guid LancamentoId { get; set; }
    public DateTime Data { get; set; }
    public decimal Valor { get; set; }
    public Guid ContaId { get; set; }
    public string? ContaNome { get; set; }
    public Guid? ComprovanteFinanceiroId { get; set; }
    public string? Observacoes { get; set; }
}
