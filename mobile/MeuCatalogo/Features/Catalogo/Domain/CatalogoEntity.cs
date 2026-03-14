using MeuCatalogo.Core.Base;
using MeuCatalogo.Domain.Enums;
using SQLite;

namespace MeuCatalogo.Features.Catalogo.Domain;

[Table("Catalogos")]
public sealed class CatalogoEntity : BaseEntity
{
    public string Nome { get; set; } = string.Empty;
    public string? NomeCurto { get; set; }
    public string? Email { get; set; }
    public string? NumeroWhatsapp { get; set; }
    public string? Descricao { get; set; }

    public SyncStatus SyncStatus { get; set; } = SyncStatus.Completed;
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    public int Version { get; set; } = 1;
    public string DeviceId { get; set; } = string.Empty;
}

