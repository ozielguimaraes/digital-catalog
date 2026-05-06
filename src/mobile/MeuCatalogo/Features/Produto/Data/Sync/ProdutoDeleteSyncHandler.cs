using MeuCatalogo.Domain.Entities;
using MeuCatalogo.Domain.Enums;
using MeuCatalogo.Features.Produto.Domain;
using MeuCatalogo.Features.Produto.UseCases;
using MeuCatalogo.Infrastructure.Database;
using MeuCatalogo.Infrastructure.SyncEngine;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Features.Produto.Data.Sync;

public sealed class ProdutoDeleteSyncHandler : ISyncHandler
{
    private readonly ILogger<ProdutoDeleteSyncHandler> _logger;
    private readonly AppDbContext _dbContext;
    private readonly DeleteProdutoRemoteUseCase _deleteRemoteUseCase;

    public ProdutoDeleteSyncHandler(
        ILogger<ProdutoDeleteSyncHandler> logger,
        AppDbContext dbContext,
        DeleteProdutoRemoteUseCase deleteRemoteUseCase)
    {
        _logger = logger;
        _dbContext = dbContext;
        _deleteRemoteUseCase = deleteRemoteUseCase;
    }

    public bool CanHandle(SyncQueue item)
        => item.EntityType == nameof(ProdutoEntity) && item.Operation == SyncOperation.Delete;

    public async Task HandleAsync(SyncQueue item, CancellationToken ct = default)
    {
        await _dbContext.InitializeAsync();

        var id = Guid.Parse(item.EntityId);
        var response = await _deleteRemoteUseCase.ExecuteAsync(id);
        if (response.RetornouComErro)
            throw new InvalidOperationException(response.MensagemDeErro ?? "Erro ao remover produto");

        await _dbContext.Database.RunInTransactionAsync(database =>
        {
            database.Execute("DELETE FROM Produtos WHERE Id = ?", item.EntityId);
            database.Execute("DELETE FROM ProdutoImagens WHERE ProdutoId = ?", item.EntityId);
        });

        _logger.LogInformation("Produto sincronizado (delete). Id={Id}", item.EntityId);
    }
}

