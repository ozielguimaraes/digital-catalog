using MeuCatalogo.Core.Base;
using MeuCatalogo.Domain.Enums;
using SQLite;

namespace MeuCatalogo.Features.Produto.Domain;

[Table("Produtos")]
public class ProdutoEntity : BaseEntity
{
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public decimal? PrecoComDesconto { get; set; }
    public string? InformacoesAdicionais { get; set; }

    public string? CatalogoId { get; set; }
    public string? CategoriaId { get; set; }
    public string? CategoriaNome { get; set; }
    public string? ThumbnailUrl { get; set; }

    // Offline Tracking Metadata
    public SyncStatus SyncStatus { get; set; } = SyncStatus.Completed;
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    public int Version { get; set; } = 1;
}
