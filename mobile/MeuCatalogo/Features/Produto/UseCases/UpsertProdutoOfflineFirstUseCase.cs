using System.Text;
using System.Text.Json;
using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Domain.Entities;
using MeuCatalogo.Domain.Enums;
using MeuCatalogo.Features.Produto.Data.Local;
using MeuCatalogo.Features.Produto.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Produto.Data.Remote.Contracts.Responses;
using MeuCatalogo.Features.Produto.Domain;
using MeuCatalogo.Features.Produto.Validators;
using MeuCatalogo.Features.Settings.Services;
using MeuCatalogo.Infrastructure.Database;
using MeuCatalogo.Infrastructure.SyncEngine;

namespace MeuCatalogo.Features.Produto.UseCases;

public sealed record UpsertProdutoOfflineFirstRequest(
    Guid? ProdutoId,
    string Nome,
    Guid CategoriaId,
    string CategoriaNome,
    decimal Preco,
    decimal? PrecoComDesconto,
    string? InformacoesAdicionais,
    IReadOnlyList<ProdutoImagemResponse> Imagens,
    SyncStatus? CurrentSyncStatus = null);

public sealed class UpsertProdutoOfflineFirstUseCase(
    IConnectivity connectivity,
    ISettingsService settingsService,
    AppDbContext dbContext,
    ISyncEngine syncEngine,
    IProdutoLocalRepository produtoLocalRepository,
    IProdutoImagemLocalRepository produtoImagemLocalRepository,
    CreateProdutoRemoteUseCase createRemoteUseCase,
    UpdateProdutoRemoteUseCase updateRemoteUseCase)
    : IUseCase<UpsertProdutoOfflineFirstRequest, ApiResponse<ProdutoResponse>>
{
    public async Task<ApiResponse<ProdutoResponse>> ExecuteAsync(UpsertProdutoOfflineFirstRequest request)
    {
        var catalogoId = settingsService.CatalogoFavorito?.Id ?? Guid.Empty;

        if (catalogoId == Guid.Empty)
            return ApiResponse<ProdutoResponse>.Error("Nenhum catálogo favorito encontrado. Selecione um catálogo antes de salvar um produto.");

        var validator = new UpsertProdutoValidator(request);
        if (!validator.IsValid)
        {
            var messages = validator.Notifications.Select(x => x.Message);
            var sb = new StringBuilder();
            foreach (string message in messages)
                sb.Append($"{message}\n");

            string error = sb.ToString().TrimEnd();
            return ApiResponse<ProdutoResponse>.Error(error);
        }

        if (connectivity.NetworkAccess == NetworkAccess.Internet)
        {
            return await UpsertOnlineAsync(request, catalogoId);
        }

        return await UpsertOfflineAsync(request, catalogoId);
    }

    private async Task<ApiResponse<ProdutoResponse>> UpsertOnlineAsync(UpsertProdutoOfflineFirstRequest request, Guid catalogoId)
    {
        ApiResponse<ProdutoResponse> response;

        if (request.ProdutoId is null)
        {
            var create = new ProdutoCreateRequest
            {
                Nome = request.Nome,
                CategoriaId = request.CategoriaId,
                CatalogoId = catalogoId,
                Preco = request.Preco,
                PrecoComDesconto = request.PrecoComDesconto,
                InformacoesAdicionais = request.InformacoesAdicionais
            };

            response = await createRemoteUseCase.ExecuteAsync(create);
        }
        else
        {
            var update = new ProdutoUpdateRequest
            {
                Nome = request.Nome,
                CategoriaId = request.CategoriaId,
                Preco = request.Preco,
                PrecoComDesconto = request.PrecoComDesconto,
                InformacoesAdicionais = request.InformacoesAdicionais,
                Imagens = request.Imagens.ToList()
            };

            response = await updateRemoteUseCase.ExecuteAsync((request.ProdutoId.Value, update));
        }

        if (response is { RetornouComSucesso: true, Dados: not null })
        {
            await PersistRemoteProdutoAsync(response.Dados);
        }

        return response;
    }

    private async Task<ApiResponse<ProdutoResponse>> UpsertOfflineAsync(UpsertProdutoOfflineFirstRequest request, Guid catalogoId)
    {
        var localId = request.ProdutoId?.ToString() ?? Guid.NewGuid().ToString();
        var now = DateTime.UtcNow;

        var entity = new ProdutoEntity
        {
            Id = localId,
            Nome = request.Nome,
            Preco = request.Preco,
            PrecoComDesconto = request.PrecoComDesconto,
            InformacoesAdicionais = request.InformacoesAdicionais,
            CatalogoId = catalogoId.ToString(),
            CategoriaId = request.CategoriaId.ToString(),
            CategoriaNome = request.CategoriaNome,
            ThumbnailUrl = request.Imagens.FirstOrDefault()?.Images?.Thumbnail ?? request.Imagens.FirstOrDefault()?.Url,
            SyncStatus = SyncStatus.Pending,
            LastModified = now
        };

        var existing = await produtoLocalRepository.GetByIdAsync(localId);
        if (existing is null)
            await produtoLocalRepository.AddAsync(entity);
        else
            await produtoLocalRepository.UpdateAsync(entity);

        var imagensEntity = request.Imagens.Select(i => new ProdutoImagemEntity
        {
            Id = i.Id.ToString(),
            ProdutoId = localId,
            CatalogoId = catalogoId.ToString(),
            Url = i.Url,
            Thumbnail = i.Images.Thumbnail,
            Medium = i.Images.Medium,
            Full = i.Images.Full,
            IsPrincipal = i.IsPrincipal,
            Ordem = i.Ordem,
            SyncStatus = i.SyncStatus,
            LastModified = now
        }).ToList();

        await produtoImagemLocalRepository.ReplaceByProdutoIdAsync(localId, imagensEntity);

        var operation = request.ProdutoId is null ? SyncOperation.Create : SyncOperation.Update;

        if (request.CurrentSyncStatus is SyncStatus.Pending)
        {
            operation = SyncOperation.Create;
            await UpsertQueuedCreateAsync(localId, entity);
        }
        else
        {
            var payload = JsonSerializer.Serialize(entity);
            await syncEngine.QueueSyncAsync(nameof(ProdutoEntity), localId, operation, payload);
        }

        var produtoResponse = new ProdutoResponse
        {
            Id = Guid.TryParse(localId, out var parsedId) ? parsedId : Guid.Empty,
            Nome = entity.Nome,
            Preco = entity.Preco,
            PrecoComDesconto = entity.PrecoComDesconto,
            InformacoesAdicionais = entity.InformacoesAdicionais,
            CategoriaId = request.CategoriaId,
            CategoriaNome = request.CategoriaNome,
            CatalogoId = catalogoId,
            Estoque = null,
            Imagens = request.Imagens.ToList(),
            SyncStatus = SyncStatus.Pending
        };

        return ApiResponse<ProdutoResponse>.Success(produtoResponse);
    }

    private async Task PersistRemoteProdutoAsync(ProdutoResponse produto)
    {
        var id = produto.Id.ToString();
        var now = DateTime.UtcNow;

        var entity = new ProdutoEntity
        {
            Id = id,
            Nome = produto.Nome,
            Preco = produto.Preco,
            PrecoComDesconto = produto.PrecoComDesconto,
            InformacoesAdicionais = produto.InformacoesAdicionais,
            CatalogoId = produto.CatalogoId.ToString(),
            CategoriaId = produto.CategoriaId.ToString(),
            CategoriaNome = produto.CategoriaNome,
            ThumbnailUrl = produto.Imagens.FirstOrDefault()?.Images?.Thumbnail ?? produto.Imagens.FirstOrDefault()?.Url,
            SyncStatus = SyncStatus.Completed,
            LastModified = now
        };

        var existing = await produtoLocalRepository.GetByIdAsync(id);
        if (existing is null)
            await produtoLocalRepository.AddAsync(entity);
        else
            await produtoLocalRepository.UpdateAsync(entity);

        var imagens = produto.Imagens.Select(i => new ProdutoImagemEntity
        {
            Id = i.Id.ToString(),
            ProdutoId = id,
            CatalogoId = produto.CatalogoId.ToString(),
            Url = i.Url,
            Thumbnail = i.Images.Thumbnail,
            Medium = i.Images.Medium,
            Full = i.Images.Full,
            IsPrincipal = i.IsPrincipal,
            Ordem = i.Ordem,
            SyncStatus = i.SyncStatus,
            LastModified = now
        }).ToList();

        await produtoImagemLocalRepository.ReplaceByProdutoIdAsync(id, imagens);
    }

    private async Task UpsertQueuedCreateAsync(string entityId, ProdutoEntity entity)
    {
        await dbContext.InitializeAsync();

        var existing = await dbContext.Database.Table<SyncQueue>()
            .Where(q => q.EntityType == nameof(ProdutoEntity) && q.EntityId == entityId && q.Operation == SyncOperation.Create)
            .FirstOrDefaultAsync();

        var payload = JsonSerializer.Serialize(entity);

        if (existing is null)
        {
            await syncEngine.QueueSyncAsync(nameof(ProdutoEntity), entityId, SyncOperation.Create, payload);
            return;
        }

        existing.Payload = payload;
        existing.Status = SyncStatus.Pending;
        existing.LastError = null;
        existing.RetryCount = 0;
        await dbContext.Database.UpdateAsync(existing);
    }
}
