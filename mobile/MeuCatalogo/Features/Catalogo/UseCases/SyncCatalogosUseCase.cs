using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Domain.Enums;
using MeuCatalogo.Infrastructure.SyncEngine;

namespace MeuCatalogo.Features.Catalogo.UseCases;

public sealed class SyncCatalogosUseCase : IUseCaseOut<int>
{
    private readonly ISyncEngine _syncEngine;

    public SyncCatalogosUseCase(ISyncEngine syncEngine)
    {
        _syncEngine = syncEngine;
    }

    public async Task<int> ExecuteAsync()
    {
        await _syncEngine.QueueSyncAsync(SyncEntityTypes.Catalogos, "all", SyncOperation.PullCatalogos, "{}");
        await _syncEngine.ProcessQueueAsync();
        return 0;
    }
}

