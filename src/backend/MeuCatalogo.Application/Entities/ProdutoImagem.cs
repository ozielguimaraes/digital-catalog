namespace MeuCatalogo.Application.Entities;

public class ProdutoImagem : BaseEntity
{
    public Guid ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;
    
    public string FileName { get; set; } = string.Empty;
    public string BasePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public bool IsPrincipal { get; set; }
    public int Ordem { get; set; }
}
