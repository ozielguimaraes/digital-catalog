namespace MeuCatalogo.Application.DTOs.Requests;

public class LancamentoBaixaRequest
{
    public DateTime Data { get; set; }
    public decimal Valor { get; set; }
    public Guid ContaId { get; set; }
    public Guid? ComprovanteFinanceiroId { get; set; }
    public string? Observacoes { get; set; }
}
