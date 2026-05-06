using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Domain.Entities;
using MeuCatalogo.Domain.Enums;
using MeuCatalogo.Features.Produto.Data.Local;
using MeuCatalogo.Features.Produto.Domain;
using MeuCatalogo.Infrastructure.Database;
using MeuCatalogo.Infrastructure.SyncEngine;

namespace MeuCatalogo.Features.Produto.UseCases;

public sealed class DeleteProdutoOfflineFirstUseCase : IUseCase<Guid, ApiResponse<Guid>>
{
    private readonly IConnectivity _connectivity;
    private readonly AppDbContext _dbContext;
    private readonly ISyncEngine _syncEngine;
    private readonly DeleteProdutoRemoteUseCase _deleteRemoteUseCase;

    public DeleteProdutoOfflineFirstUseCase(
        IConnectivity connectivity,
        AppDbContext dbContext,
        ISyncEngine syncEngine,
        DeleteProdutoRemoteUseCase deleteRemoteUseCase)
    {
        _connectivity = connectivity;
        _dbContext = dbContext;
        _syncEngine = syncEngine;
        _deleteRemoteUseCase = deleteRemoteUseCase;
    }

    public async Task<ApiResponse<Guid>> ExecuteAsync(Guid request)
    {
        await _dbContext.InitializeAsync();

        var id = request.ToString();

        var entity = await _dbContext.Database.Table<ProdutoEntity>()
            .Where(p => p.Id == id)
            .FirstOrDefaultAsync();

        if (entity == null)
            return ApiResponse<Guid>.Success(request);

        if (_connectivity.NetworkAccess == NetworkAccess.Internet)
        {
            var remote = await _deleteRemoteUseCase.ExecuteAsync(request);
            if (remote.RetornouComSucesso)
            {
                await DeleteLocalAsync(id);
                await DeleteQueuedItemsAsync(id);
                return remote;
            }
        }

        await DeleteLocalAsync(id);

        var hasQueuedCreate = await _dbContext.Database.Table<SyncQueue>()
            .Where(q => q.EntityType == nameof(ProdutoEntity) && q.EntityId == id && q.Operation == SyncOperation.Create)
            .CountAsync() > 0;

        if (hasQueuedCreate)
        {
            await DeleteQueuedItemsAsync(id);
            return ApiResponse<Guid>.Success(request);
        }

        await _syncEngine.QueueSyncAsync(nameof(ProdutoEntity), id, SyncOperation.Delete, "{}");
        return ApiResponse<Guid>.Success(request);
    }

    private async Task DeleteLocalAsync(string id)
    {
        await _dbContext.Database.RunInTransactionAsync(database =>
        {
            database.Execute("DELETE FROM Produtos WHERE Id = ?", id);
            database.Execute("DELETE FROM ProdutoImagens WHERE ProdutoId = ?", id);
        });
    }

    private async Task DeleteQueuedItemsAsync(string id)
    {
        await _dbContext.Database.ExecuteAsync(
            "DELETE FROM SyncQueues WHERE EntityType = ? AND EntityId = ?",
            nameof(ProdutoEntity),
            id);
    }
}
