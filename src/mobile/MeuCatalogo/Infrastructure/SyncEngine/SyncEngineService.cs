using MeuCatalogo.Domain.Entities;
using MeuCatalogo.Domain.Enums;
using MeuCatalogo.Infrastructure.Database;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading;

namespace MeuCatalogo.Infrastructure.SyncEngine;

public sealed class SyncEngineService(
    AppDbContext dbContext,
    ILogger<SyncEngineService> logger,
    IConnectivity connectivity,
    IEnumerable<ISyncHandler> handlers)
    : ISyncEngine
{
    private readonly IReadOnlyList<ISyncHandler> _handlers = handlers.ToList();

    private static readonly SemaphoreSlim _syncLock = new(1, 1);

    public async Task QueueSyncAsync(
        string entityType,
        string entityId,
        SyncOperation operation,
        string payload)
    {
        await dbContext.InitializeAsync();

        var syncItem = new SyncQueue
        {
            EntityType = entityType,
            EntityId = entityId,
            Operation = operation,
            Payload = payload,
            Status = SyncStatus.Pending,
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow,
            NextRetryAt = DateTime.UtcNow
        };

        await dbContext.Database.InsertAsync(syncItem);

        logger.LogInformation(
            "Queued sync item {Id} ({EntityType}/{Operation})",
            syncItem.Id,
            entityType,
            operation);

        if (connectivity.NetworkAccess == NetworkAccess.Internet)
        {
            _ = TriggerProcessingAsync();
        }
    }

    private async Task TriggerProcessingAsync()
    {
        if (!await _syncLock.WaitAsync(0))
            return;

        try
        {
            await ProcessQueueAsync();
        }
        finally
        {
            _syncLock.Release();
        }
    }

    public async Task ProcessQueueAsync()
    {
        if (connectivity.NetworkAccess != NetworkAccess.Internet)
        {
            logger.LogWarning("No internet. Skipping sync.");
            return;
        }

        await dbContext.InitializeAsync();

        var now = DateTime.UtcNow;

        var pendingItems = await dbContext.Database.Table<SyncQueue>()
            .Where(q =>
                (q.Status == SyncStatus.Pending || q.Status == SyncStatus.Failed) &&
                q.NextRetryAt <= now)
            .OrderBy(q => q.CreatedAt)
            .ToListAsync();

        if (pendingItems.Count == 0)
            return;

        foreach (var item in pendingItems)
        {
            try
            {
                item.Status = SyncStatus.InProgress;
                await dbContext.Database.UpdateAsync(item);

                var success = await ProcessItemAsync(item);

                if (success)
                {
                    item.Status = SyncStatus.Completed;
                    item.CompletedAt = DateTime.UtcNow;

                    await dbContext.Database.UpdateAsync(item);
                }
                else
                {
                    await MarkAsFailedAsync(item, "Handler returned false");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Error processing sync item {Id}", item.Id);

                await MarkAsFailedAsync(item, ex.Message);
            }
        }
    }

    private async Task<bool> ProcessItemAsync(SyncQueue item)
    {
        var handler = _handlers.FirstOrDefault(h => h.CanHandle(item));

        if (handler is null)
        {
            logger.LogWarning(
                "No handler for {EntityType}/{Operation}",
                item.EntityType,
                item.Operation);

            return false;
        }

        await handler.HandleAsync(item);
        return true;
    }

    private async Task MarkAsFailedAsync(SyncQueue item, string error)
    {
        item.Status = SyncStatus.Failed;
        item.RetryCount++;
        item.LastError = error;

        // Backoff exponencial (máx 5 min)
        var delaySeconds = Math.Min(Math.Pow(2, item.RetryCount), 300);

        item.NextRetryAt = DateTime.UtcNow.AddSeconds(delaySeconds);

        await dbContext.Database.UpdateAsync(item);
    }
}
