using System.Text.Json;
using MeuCatalogo.Domain.Entities;
using MeuCatalogo.Domain.Enums;
using MeuCatalogo.Features.Categoria.Data.Local;
using MeuCatalogo.Features.Categoria.Data.Remote;
using MeuCatalogo.Features.Categoria.Domain;
using MeuCatalogo.Infrastructure.SyncEngine;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Features.Categoria.Data.Sync;

public sealed class CategoriaPullSyncHandler(
    ILogger<CategoriaPullSyncHandler> logger,
    ICategoriaRemoteDataSource remote,
    ICategoriaLocalRepository local)
    : ISyncHandler
{
    public bool CanHandle(SyncQueue item)
        => item.Operation == SyncOperation.PullCategoriasByCatalogoId && item.EntityType == SyncEntityTypes.CategoriasByCatalogo;

    public async Task HandleAsync(SyncQueue item, CancellationToken ct = default)
    {
        var payload = JsonSerializer.Deserialize<PullCategoriasByCatalogoPayload>(item.Payload);
        if (payload == null || string.IsNullOrWhiteSpace(payload.CatalogoId))
            throw new InvalidOperationException("Payload inválido para sincronização de categorias");

        var catalogoId = Guid.Parse(payload.CatalogoId);
        var response = await remote.GetByCatalogoIdAsync(catalogoId, ct);
        if (response.RetornouComErro || response.Dados == null)
            throw new InvalidOperationException(response.MensagemDeErro ?? "Erro ao sincronizar categorias");

        var now = DateTime.UtcNow;

        var entities = response.Dados.Select(c => new CategoriaEntity
        {
            Id = c.Id.ToString(),
            Nome = c.Nome,
            Descricao = c.Descricao,
            CatalogoId = c.CatalogoId.ToString(),
            SyncStatus = SyncStatus.Completed,
            LastModified = now
        }).ToList();

        await local.ReplaceByCatalogoIdAsync(payload.CatalogoId, entities);
        logger.LogInformation("Categorias sincronizadas para catálogo {CatalogoId}: {Count}", payload.CatalogoId, entities.Count);
    }

    private sealed record PullCategoriasByCatalogoPayload
    {
        public required string CatalogoId { get; init; }
    }
}

