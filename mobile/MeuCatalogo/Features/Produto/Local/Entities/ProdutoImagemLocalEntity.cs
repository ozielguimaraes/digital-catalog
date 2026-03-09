using SQLite;

namespace MeuCatalogo.Features.Produto.Local.Entities;

[Table("produto_imagens")]
public sealed class ProdutoImagemLocalEntity
{
    [PrimaryKey]
    public Guid Id { get; set; }

    [Indexed]
    public Guid ProdutoId { get; set; }

    public string Url { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string MediumUrl { get; set; } = string.Empty;
    public string FullUrl { get; set; } = string.Empty;
    public bool IsPrincipal { get; set; }
    public int Ordem { get; set; }
    public int SyncStatus { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
