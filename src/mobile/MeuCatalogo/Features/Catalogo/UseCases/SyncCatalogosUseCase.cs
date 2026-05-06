using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Domain.Enums;
using MeuCatalogo.Infrastructure.SyncEngine;

namespace MeuCatalogo.Features.Catalogo.UseCases;

public sealed class SyncCatalogosUseCase(ISyncEngine syncEngine) : IUseCaseOut<int>
{
    public async Task<int> ExecuteAsync()
    {
        await syncEngine.QueueSyncAsync(SyncEntityTypes.Catalogos, "all", SyncOperation.PullCatalogos, "{}");
        await syncEngine.ProcessQueueAsync();
        return 0;
    }
}

