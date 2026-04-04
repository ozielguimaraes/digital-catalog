using MeuCatalogo.Core.Base;
using MeuCatalogo.Domain.Enums;
using SQLite;

namespace MeuCatalogo.Domain.Entities;

[Table("SyncQueues")]
public class SyncQueue : BaseEntity
{
    [Indexed]
    public string EntityType { get; set; } = string.Empty;

    [Indexed]
    public string EntityId { get; set; } = string.Empty;

    public SyncOperation Operation { get; set; }

    // JSON serialized payload of the operation
    public string Payload { get; set; } = string.Empty;

    [Indexed]
    public SyncStatus Status { get; set; } = SyncStatus.Pending;

    public int RetryCount { get; set; } = 0;

    public string? LastError { get; set; }

    [Indexed]
    public DateTime? NextRetryAt { get; set; }

        public DateTime? CompletedAt { get; set; }
}
