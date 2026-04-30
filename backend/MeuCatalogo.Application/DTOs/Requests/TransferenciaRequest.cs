namespace MeuCatalogo.Application.DTOs.Requests;

public class TransferenciaRequest
{
    public DateTime Data { get; set; }
    public Guid ContaOrigemId { get; set; }
    public Guid ContaDestinoId { get; set; }
    public decimal Valor { get; set; }
    public string? Descricao { get; set; }
    public int? FaturaMes { get; set; }
    public int? FaturaAno { get; set; }
}
