using MeuCatalogo.Domain.Entities;
using MeuCatalogo.Domain.Enums;
using MeuCatalogo.Infrastructure.Database;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Infrastructure.SyncEngine;

public class SyncEngineService(
    AppDbContext dbContext,
    ILogger<SyncEngineService> logger,
    IConnectivity connectivity,
    IEnumerable<ISyncHandler> handlers)
    : ISyncEngine
{
    private readonly IReadOnlyList<ISyncHandler> _handlers = handlers.ToList();

    public async Task QueueSyncAsync(string entityType, string entityId, SyncOperation operation, string payload)
    {
        await dbContext.InitializeAsync();

        var syncItem = new SyncQueue
        {
            EntityType = entityType,
            EntityId = entityId,
            Operation = operation,
            Payload = payload,
            Status = SyncStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await dbContext.Database.InsertAsync(syncItem);
        logger.LogInformation($"Queued sync item {syncItem.Id} for {entityType}");

        // Attempt immediate processing if connected
        if (connectivity.NetworkAccess == NetworkAccess.Internet)
        {
            _ = Task.Run(ProcessQueueAsync);
        }
    }

    public async Task ProcessQueueAsync()
    {
        if (connectivity.NetworkAccess != NetworkAccess.Internet)
        {
            logger.LogWarning("Cannot process sync queue. No internet connection.");
            return;
        }

        await dbContext.InitializeAsync();

        var pendingItems = await dbContext.Database.Table<SyncQueue>()
            .Where(q => q.Status == SyncStatus.Pending || q.Status == SyncStatus.Failed)
            .OrderBy(q => q.CreatedAt)
            .ToListAsync();

        if (!pendingItems.Any())
            return;

        foreach (var item in pendingItems)
        {
            try
            {
                item.Status = SyncStatus.InProgress;
                await dbContext.Database.UpdateAsync(item);

                bool success = await ProcessItemAsync(item);

                if (success)
                {
                    item.Status = SyncStatus.Completed;
                    await dbContext.Database.DeleteAsync(item);
                }
                else
                {
                    item.Status = SyncStatus.Failed;
                    item.RetryCount++;
                    await dbContext.Database.UpdateAsync(item);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error processing sync queue item {item.Id}");
                item.Status = SyncStatus.Failed;
                item.RetryCount++;
                item.LastError = ex.Message;
                await dbContext.Database.UpdateAsync(item);
            }
        }
    }

    private async Task<bool> ProcessItemAsync(SyncQueue item)
    {
        var handler = _handlers.FirstOrDefault(h => h.CanHandle(item));
        if (handler == null)
        {
            logger.LogWarning("No handler registered for sync item {EntityType} / {Operation}", item.EntityType, item.Operation);
            return false;
        }

        await handler.HandleAsync(item);
        return true;
    }
}
