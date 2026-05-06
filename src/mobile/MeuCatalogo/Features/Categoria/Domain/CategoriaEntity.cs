using MeuCatalogo.Core.Base;
using MeuCatalogo.Domain.Enums;
using SQLite;

namespace MeuCatalogo.Features.Categoria.Domain;

[Table("Categorias")]
public sealed class CategoriaEntity : BaseEntity
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string CatalogoId { get; set; } = string.Empty;

    public SyncStatus SyncStatus { get; set; } = SyncStatus.Completed;
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    public int Version { get; set; } = 1;
}

