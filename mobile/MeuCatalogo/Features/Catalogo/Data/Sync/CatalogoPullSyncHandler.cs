using MeuCatalogo.Domain.Entities;
using MeuCatalogo.Domain.Enums;
using MeuCatalogo.Features.Catalogo.Data.Local;
using MeuCatalogo.Features.Catalogo.Data.Remote;
using MeuCatalogo.Features.Catalogo.Domain;
using MeuCatalogo.Infrastructure.SyncEngine;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Features.Catalogo.Data.Sync;

public sealed class CatalogoPullSyncHandler : ISyncHandler
{
    private readonly ILogger<CatalogoPullSyncHandler> _logger;
    private readonly ICatalogoRemoteDataSource _remote;
    private readonly ICatalogoLocalRepository _local;

    public CatalogoPullSyncHandler(
        ILogger<CatalogoPullSyncHandler> logger,
        ICatalogoRemoteDataSource remote,
        ICatalogoLocalRepository local)
    {
        _logger = logger;
        _remote = remote;
        _local = local;
    }

    public bool CanHandle(SyncQueue item)
        => item.Operation == SyncOperation.PullCatalogos && item.EntityType == SyncEntityTypes.Catalogos;

    public async Task HandleAsync(SyncQueue item, CancellationToken ct = default)
    {
        var response = await _remote.GetCatalogosAsync(ct);
        if (response.RetornouComErro || response.Dados == null)
            throw new InvalidOperationException(response.MensagemDeErro ?? "Erro ao sincronizar catálogos");

        var now = DateTime.UtcNow;

        var entities = response.Dados.Select(c => new CatalogoEntity
        {
            Id = c.Id.ToString(),
            Nome = c.Nome,
            NomeCurto = c.NomeCurto,
            Email = c.Email,
            NumeroWhatsapp = c.NumeroWhatsapp,
            Descricao = c.Descricao,
            SyncStatus = SyncStatus.Completed,
            LastModified = now
        }).ToList();

        await _local.ReplaceAllAsync(entities);
        _logger.LogInformation("Catálogos sincronizados: {Count}", entities.Count);
    }
}
