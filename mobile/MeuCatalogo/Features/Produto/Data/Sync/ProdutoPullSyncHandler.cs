using System.Linq;
using System.Text.Json;
using MeuCatalogo.Domain.Entities;
using MeuCatalogo.Domain.Enums;
using MeuCatalogo.Features.Produto.Data;
using MeuCatalogo.Features.Produto.Data.Local;
using MeuCatalogo.Features.Produto.Domain;
using MeuCatalogo.Infrastructure.SyncEngine;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Features.Produto.Data.Sync;

public sealed class ProdutoPullSyncHandler(
    ILogger<ProdutoPullSyncHandler> logger,
    IProdutoRepository remoteRepository,
    IProdutoLocalRepository localRepository,
    IProdutoImagemLocalRepository imagemLocalRepository)
    : ISyncHandler
{
    public bool CanHandle(SyncQueue item)
        => item.Operation == SyncOperation.PullProdutosByCatalogoId && item.EntityType == SyncEntityTypes.ProdutosByCatalogo;

    public async Task HandleAsync(SyncQueue item, CancellationToken ct = default)
    {
        var payload = JsonSerializer.Deserialize<PullProdutosByCatalogoPayload>(item.Payload);
        if (payload == null || string.IsNullOrWhiteSpace(payload.CatalogoId))
            throw new InvalidOperationException("Payload inválido para sincronização de produtos");

        var catalogoId = Guid.Parse(payload.CatalogoId);
        var response = await remoteRepository.ObterPorCatalogoIdAsync(catalogoId, ct);
        if (response.RetornouComErro || response.Dados == null)
            throw new InvalidOperationException(response.MensagemDeErro ?? "Erro ao sincronizar produtos");

        var now = DateTime.UtcNow;
        var entities = response.Dados.Select(p => new ProdutoEntity
        {
            Id = p.Id.ToString(),
            Nome = p.Nome,
            Preco = p.Preco,
            PrecoComDesconto = p.PrecoComDesconto,
            InformacoesAdicionais = p.InformacoesAdicionais,
            CatalogoId = p.CatalogoId.ToString(),
            CategoriaId = p.CategoriaId.ToString(),
            CategoriaNome = p.CategoriaNome,
            ThumbnailUrl = p.Imagens.FirstOrDefault()?.Images?.Thumbnail ?? p.Imagens.FirstOrDefault()?.Url,
            SyncStatus = SyncStatus.Completed,
            LastModified = now
        }).ToList();

        await localRepository.ReplaceByCatalogoIdAsync(payload.CatalogoId, entities);

        var imagens = response.Dados
            .SelectMany(p => p.Imagens.Select(img => new ProdutoImagemEntity
            {
                Id = img.Id.ToString(),
                ProdutoId = p.Id.ToString(),
                CatalogoId = p.CatalogoId.ToString(),
                Url = img.Url,
                Thumbnail = img.Images.Thumbnail,
                Medium = img.Images.Medium,
                Full = img.Images.Full,
                IsPrincipal = img.IsPrincipal,
                Ordem = img.Ordem,
                SyncStatus = img.SyncStatus,
                LastModified = now
            }))
            .ToList();

        await imagemLocalRepository.ReplaceByCatalogoIdAsync(payload.CatalogoId, imagens);
        logger.LogInformation("Produtos sincronizados para catálogo {CatalogoId}: {Count}", payload.CatalogoId, entities.Count);
    }

    private sealed record PullProdutosByCatalogoPayload
    {
        public required string CatalogoId { get; init; }
    }
}
