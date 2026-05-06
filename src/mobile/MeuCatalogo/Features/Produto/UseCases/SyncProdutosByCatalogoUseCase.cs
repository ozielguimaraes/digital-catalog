using System.Text.Json;
using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Domain.Enums;
using MeuCatalogo.Features.Produto.Data.Local;
using MeuCatalogo.Infrastructure.SyncEngine;

namespace MeuCatalogo.Features.Produto.UseCases;

public sealed class SyncProdutosByCatalogoUseCase(ISyncEngine syncEngine, IProdutoLocalRepository localRepository)
    : IUseCase<Guid, int>
{
    public async Task<int> ExecuteAsync(Guid request)
    {
        var payload = JsonSerializer.Serialize(new PullProdutosByCatalogoPayload { CatalogoId = request.ToString() });
        await syncEngine.QueueSyncAsync(SyncEntityTypes.ProdutosByCatalogo, request.ToString(), SyncOperation.PullProdutosByCatalogoId, payload);
        await syncEngine.ProcessQueueAsync();

        var produtos = await localRepository.GetByCatalogoIdAsync(request.ToString());
        return produtos.Count();
    }

    private sealed record PullProdutosByCatalogoPayload
    {
        public required string CatalogoId { get; init; }
    }
}

