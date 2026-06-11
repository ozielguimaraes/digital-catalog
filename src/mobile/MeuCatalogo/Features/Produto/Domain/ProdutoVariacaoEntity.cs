using MeuCatalogo.Core.Base;
using MeuCatalogo.Domain.Enums;
using SQLite;

namespace MeuCatalogo.Features.Produto.Domain;

[Table("ProdutoVariacoes")]
public sealed class ProdutoVariacaoEntity : BaseEntity
{
    [Indexed]
    public string ProdutoId { get; set; } = string.Empty;

    [Indexed]
    public string CatalogoId { get; set; } = string.Empty;

    public string? Cor { get; set; }
    public string? Tamanho { get; set; }
    public decimal? Preco { get; set; }
    public int Quantidade { get; set; }

    public SyncStatus SyncStatus { get; set; } = SyncStatus.Completed;
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    public int Version { get; set; } = 1;
}
