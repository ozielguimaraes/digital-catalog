using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Domain.Enums;
using MeuCatalogo.Features.Produto.Data.Local;
using MeuCatalogo.Features.Produto.Domain;
using MeuCatalogo.Infrastructure.SyncEngine;
using System.Text.Json;

namespace MeuCatalogo.Features.Produto.UseCases;

public class CreateProdutoRequest
{
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public string? CategoriaId { get; set; }
}

public class CreateProdutoUseCase(IProdutoLocalRepository repository, ISyncEngine syncEngine)
    : IUseCase<CreateProdutoRequest, ProdutoEntity>
{
    public async Task<ProdutoEntity> ExecuteAsync(CreateProdutoRequest request)
    {
        // 1. Create purely local entity
        var entity = new ProdutoEntity
        {
            Nome = request.Nome,
            Preco = request.Preco,
            CategoriaId = request.CategoriaId,
            SyncStatus = SyncStatus.Pending, // Important: Mark as pending sync
            LastModified = DateTime.UtcNow,
            // DeviceId = ... get from some device Info service
        };

        // 2. Save directly to Local Database (Fast, Offline)
        await repository.AddAsync(entity);

        // 3. Queue for synchronization to Remote API
        var payload = JsonSerializer.Serialize(entity);
        await syncEngine.QueueSyncAsync(nameof(ProdutoEntity), entity.Id, SyncOperation.Create, payload);

        // 4. Return result immediately
        return entity;
    }
}
