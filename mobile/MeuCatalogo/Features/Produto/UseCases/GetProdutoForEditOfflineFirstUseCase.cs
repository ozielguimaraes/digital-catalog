using System;
using System.Linq;
using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Domain.Enums;
using MeuCatalogo.Features.Produto.Data.Local;
using MeuCatalogo.Features.Produto.Data.Remote.Contracts.Responses;
using MeuCatalogo.Features.Produto.Domain;
using Microsoft.Maui.Networking;

namespace MeuCatalogo.Features.Produto.UseCases;

public sealed class GetProdutoForEditOfflineFirstUseCase : IUseCase<ProdutoEntity, ProdutoResponse>
{
    private readonly IConnectivity _connectivity;
    private readonly IProdutoLocalRepository _produtoLocalRepository;
    private readonly IProdutoImagemLocalRepository _produtoImagemLocalRepository;
    private readonly GetProdutoByIdUseCase _getProdutoByIdUseCase;

    public GetProdutoForEditOfflineFirstUseCase(
        IConnectivity connectivity,
        IProdutoLocalRepository produtoLocalRepository,
        IProdutoImagemLocalRepository produtoImagemLocalRepository,
        GetProdutoByIdUseCase getProdutoByIdUseCase)
    {
        _connectivity = connectivity;
        _produtoLocalRepository = produtoLocalRepository;
        _produtoImagemLocalRepository = produtoImagemLocalRepository;
        _getProdutoByIdUseCase = getProdutoByIdUseCase;
    }

    public async Task<ProdutoResponse> ExecuteAsync(ProdutoEntity request)
    {
        var imagens = await _produtoImagemLocalRepository.GetByProdutoIdAsync(request.Id);

        if (imagens.Count == 0 && _connectivity.NetworkAccess == NetworkAccess.Internet && Guid.TryParse(request.Id, out var remoteId) && remoteId != Guid.Empty)
        {
            var remote = await _getProdutoByIdUseCase.ExecuteAsync(remoteId);
            if (remote is { RetornouComSucesso: true, Dados: not null })
            {
                var now = DateTime.UtcNow;
                var imagensRemote = remote.Dados.Imagens.Select(i => new ProdutoImagemEntity
                {
                    Id = i.Id.ToString(),
                    ProdutoId = request.Id,
                    CatalogoId = remote.Dados.CatalogoId.ToString(),
                    Url = i.Url,
                    Thumbnail = i.Images.Thumbnail,
                    Medium = i.Images.Medium,
                    Full = i.Images.Full,
                    IsPrincipal = i.IsPrincipal,
                    Ordem = i.Ordem,
                    SyncStatus = i.SyncStatus,
                    LastModified = now
                }).ToList();

                await _produtoImagemLocalRepository.ReplaceByProdutoIdAsync(request.Id, imagensRemote);
                imagens = imagensRemote;

                if (string.IsNullOrWhiteSpace(request.ThumbnailUrl))
                {
                    request.ThumbnailUrl = remote.Dados.Imagens.FirstOrDefault()?.Images?.Thumbnail ?? remote.Dados.Imagens.FirstOrDefault()?.Url;
                    request.LastModified = now;
                    request.SyncStatus = SyncStatus.Completed;
                    await _produtoLocalRepository.UpdateAsync(request);
                }
            }
        }

        var catalogoId = Guid.TryParse(request.CatalogoId, out var parsedCatalogoId) ? parsedCatalogoId : Guid.Empty;
        var categoriaId = Guid.TryParse(request.CategoriaId, out var parsedCategoriaId) ? parsedCategoriaId : Guid.Empty;
        var produtoId = Guid.TryParse(request.Id, out var parsedProdutoId) ? parsedProdutoId : Guid.Empty;

        var imagensResponse = imagens.Select(i => new ProdutoImagemResponse
        {
            Id = Guid.TryParse(i.Id, out var imageId) ? imageId : Guid.Empty,
            Url = i.Url,
            Images = new ProdutoImagemLinksResponse
            {
                Thumbnail = i.Thumbnail,
                Medium = i.Medium,
                Full = i.Full
            },
            IsPrincipal = i.IsPrincipal,
            Ordem = i.Ordem,
            SyncStatus = i.SyncStatus
        }).ToList();

        return new ProdutoResponse
        {
            Id = produtoId,
            Nome = request.Nome,
            Preco = request.Preco,
            PrecoComDesconto = request.PrecoComDesconto,
            InformacoesAdicionais = request.InformacoesAdicionais,
            CategoriaId = categoriaId,
            CategoriaNome = request.CategoriaNome ?? string.Empty,
            CatalogoId = catalogoId,
            Estoque = null,
            Imagens = imagensResponse,
            SyncStatus = SyncStatus.Completed
        };
    }
}
