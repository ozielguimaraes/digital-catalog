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

public class CreateProdutoUseCase(
    IProdutoLocalRepository repository,
    ISyncEngine syncEngine)
    : IUseCase<CreateProdutoRequest, ProdutoEntity>
{
    public async Task<ProdutoEntity> ExecuteAsync(CreateProdutoRequest request)
    {
        var entity = new ProdutoEntity
        {
            Nome = request.Nome,
            Preco = request.Preco,
            CategoriaId = request.CategoriaId,
            SyncStatus = SyncStatus.Pending,
            LastModified = DateTime.UtcNow
        };

        await repository.AddAsync(entity);

        var payload = JsonSerializer.Serialize(entity);
        await syncEngine.QueueSyncAsync(nameof(ProdutoEntity), entity.Id, SyncOperation.Create, payload);

        return entity;
    }
}
