namespace MeuCatalogo.Application.DTOs.Responses;

public class ComprovanteFinanceiroResponse
{
    public Guid Id { get; set; }
    public string? Descricao { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Url { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
