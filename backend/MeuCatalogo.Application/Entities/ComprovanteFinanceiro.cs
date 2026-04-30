namespace MeuCatalogo.Application.Entities;

public class ComprovanteFinanceiro : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public string? Descricao { get; set; }
    public string BasePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string FileName { get; set; } = string.Empty;
}
