using MeuCatalogo.Core.Base;
using MeuCatalogo.Domain.Enums;
using SQLite;

namespace MeuCatalogo.Features.Produto.Domain;

[Table("ProdutoImagens")]
public sealed class ProdutoImagemEntity : BaseEntity
{
    [Indexed]
    public string ProdutoId { get; set; } = string.Empty;

    [Indexed]
    public string CatalogoId { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;
    public string Thumbnail { get; set; } = string.Empty;
    public string Medium { get; set; } = string.Empty;
    public string Full { get; set; } = string.Empty;
    public bool IsPrincipal { get; set; }
    public int Ordem { get; set; }

    public SyncStatus SyncStatus { get; set; } = SyncStatus.Completed;
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    public int Version { get; set; } = 1;
}

