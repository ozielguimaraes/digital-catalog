using System.Text.Json;
using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Domain.Enums;
using MeuCatalogo.Features.Produto.Data.Local;
using MeuCatalogo.Infrastructure.SyncEngine;

namespace MeuCatalogo.Features.Produto.UseCases;

public sealed class SyncProdutosByCatalogoUseCase : IUseCase<Guid, int>
{
    private readonly ISyncEngine _syncEngine;
    private readonly IProdutoLocalRepository _localRepository;

    public SyncProdutosByCatalogoUseCase(ISyncEngine syncEngine, IProdutoLocalRepository localRepository)
    {
        _syncEngine = syncEngine;
        _localRepository = localRepository;
    }

    public async Task<int> ExecuteAsync(Guid request)
    {
        var payload = JsonSerializer.Serialize(new PullProdutosByCatalogoPayload { CatalogoId = request.ToString() });
        await _syncEngine.QueueSyncAsync(SyncEntityTypes.ProdutosByCatalogo, request.ToString(), SyncOperation.PullProdutosByCatalogoId, payload);
        await _syncEngine.ProcessQueueAsync();

        var produtos = await _localRepository.GetByCatalogoIdAsync(request.ToString());
        return produtos.Count();
    }

    private sealed record PullProdutosByCatalogoPayload
    {
        public required string CatalogoId { get; init; }
    }
}

