using MeuCatalogo.Domain.Entities;
using MeuCatalogo.Domain.Enums;

namespace MeuCatalogo.Infrastructure.SyncEngine;

public interface ISyncEngine
{
    Task QueueSyncAsync(string entityType, string entityId, SyncOperation operation, string payload);
    Task ProcessQueueAsync();
}
