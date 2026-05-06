using System.Text.Json;
using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Domain.Enums;
using MeuCatalogo.Features.Categoria.Data.Local;
using MeuCatalogo.Infrastructure.SyncEngine;

namespace MeuCatalogo.Features.Categoria.UseCases;

public sealed class SyncCategoriasByCatalogoUseCase(
    ISyncEngine syncEngine,
    ICategoriaLocalRepository localRepository)
    : IUseCase<Guid, int>
{
    public async Task<int> ExecuteAsync(Guid request)
    {
        var payload = JsonSerializer.Serialize(new PullCategoriasByCatalogoPayload { CatalogoId = request.ToString() });
        await syncEngine.QueueSyncAsync(SyncEntityTypes.CategoriasByCatalogo, request.ToString(), SyncOperation.PullCategoriasByCatalogoId, payload);
        await syncEngine.ProcessQueueAsync();

        var categorias = await localRepository.GetByCatalogoIdAsync(request.ToString());
        return categorias.Count;
    }

    private sealed record PullCategoriasByCatalogoPayload
    {
        public required string CatalogoId { get; init; }
    }
}

