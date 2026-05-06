using System.Text.Json;
using MeuCatalogo.Domain.Entities;
using MeuCatalogo.Domain.Enums;
using MeuCatalogo.Features.Produto.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Produto.Domain;
using MeuCatalogo.Features.Produto.UseCases;
using MeuCatalogo.Infrastructure.Database;
using MeuCatalogo.Infrastructure.SyncEngine;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Features.Produto.Data.Sync;

public sealed class ProdutoUpsertSyncHandler(
    ILogger<ProdutoUpsertSyncHandler> logger,
    AppDbContext dbContext,
    CreateProdutoRemoteUseCase createRemoteUseCase,
    UpdateProdutoRemoteUseCase updateRemoteUseCase)
    : ISyncHandler
{
    public bool CanHandle(SyncQueue item)
        => item.EntityType == nameof(ProdutoEntity) && (item.Operation == SyncOperation.Create || item.Operation == SyncOperation.Update);

    public async Task HandleAsync(SyncQueue item, CancellationToken ct = default)
    {
        var entity = JsonSerializer.Deserialize<ProdutoEntity>(item.Payload);
        if (entity == null)
            throw new InvalidOperationException("Payload inválido para sincronização de produto");

        await dbContext.InitializeAsync();

        if (item.Operation == SyncOperation.Create)
        {
            var create = new ProdutoCreateRequest
            {
                Nome = entity.Nome,
                CategoriaId = Guid.Parse(entity.CategoriaId ?? Guid.Empty.ToString()),
                CatalogoId = Guid.Parse(entity.CatalogoId ?? Guid.Empty.ToString()),
                Preco = entity.Preco,
                PrecoComDesconto = entity.PrecoComDesconto,
                InformacoesAdicionais = entity.InformacoesAdicionais
            };

            var response = await createRemoteUseCase.ExecuteAsync(create);
            if (response.RetornouComErro || response.Dados == null)
                throw new InvalidOperationException(response.MensagemDeErro ?? "Erro ao sincronizar produto");

            var remoteId = response.Dados.Id.ToString();
            var localId = entity.Id;

            var local = await dbContext.Database.Table<ProdutoEntity>().Where(p => p.Id == localId).FirstOrDefaultAsync();
            if (local == null)
                return;

            local.Nome = response.Dados.Nome;
            local.Preco = response.Dados.Preco;
            local.PrecoComDesconto = response.Dados.PrecoComDesconto;
            local.InformacoesAdicionais = response.Dados.InformacoesAdicionais;
            local.CatalogoId = response.Dados.CatalogoId.ToString();
            local.CategoriaId = response.Dados.CategoriaId.ToString();
            local.CategoriaNome = response.Dados.CategoriaNome;
            local.ThumbnailUrl = response.Dados.Imagens.FirstOrDefault()?.Images?.Thumbnail ?? response.Dados.Imagens.FirstOrDefault()?.Url;
            local.SyncStatus = SyncStatus.Completed;
            local.LastModified = DateTime.UtcNow;

            if (remoteId != localId)
            {
                await dbContext.Database.RunInTransactionAsync(database =>
                {
                    database.Execute("UPDATE ProdutoImagens SET ProdutoId = ? WHERE ProdutoId = ?", remoteId, localId);
                    database.Execute("UPDATE ProdutoImagens SET CatalogoId = ? WHERE ProdutoId = ?", local.CatalogoId, remoteId);
                    database.Delete<ProdutoEntity>(localId);
                    local.Id = remoteId;
                    database.Insert(local);
                });
            }
            else
            {
                await dbContext.Database.UpdateAsync(local);
            }

            logger.LogInformation("Produto sincronizado (create). LocalId={LocalId} RemoteId={RemoteId}", localId, remoteId);
            return;
        }

        var updateId = Guid.Parse(entity.Id);
        var update = new ProdutoUpdateRequest
        {
            Nome = entity.Nome,
            CategoriaId = Guid.Parse(entity.CategoriaId ?? Guid.Empty.ToString()),
            Preco = entity.Preco,
            PrecoComDesconto = entity.PrecoComDesconto,
            InformacoesAdicionais = entity.InformacoesAdicionais
        };

        var updateResponse = await updateRemoteUseCase.ExecuteAsync((updateId, update));
        if (updateResponse.RetornouComErro || updateResponse.Dados == null)
            throw new InvalidOperationException(updateResponse.MensagemDeErro ?? "Erro ao sincronizar produto");

        var existing = await dbContext.Database.Table<ProdutoEntity>().Where(p => p.Id == entity.Id).FirstOrDefaultAsync();
        if (existing == null)
            return;

        existing.Nome = updateResponse.Dados.Nome;
        existing.Preco = updateResponse.Dados.Preco;
        existing.PrecoComDesconto = updateResponse.Dados.PrecoComDesconto;
        existing.InformacoesAdicionais = updateResponse.Dados.InformacoesAdicionais;
        existing.CategoriaId = updateResponse.Dados.CategoriaId.ToString();
        existing.CategoriaNome = updateResponse.Dados.CategoriaNome;
        existing.ThumbnailUrl = updateResponse.Dados.Imagens.FirstOrDefault()?.Images?.Thumbnail ?? updateResponse.Dados.Imagens.FirstOrDefault()?.Url;
        existing.SyncStatus = SyncStatus.Completed;
        existing.LastModified = DateTime.UtcNow;

        await dbContext.Database.UpdateAsync(existing);
        logger.LogInformation("Produto sincronizado (update). Id={Id}", entity.Id);
    }
}

