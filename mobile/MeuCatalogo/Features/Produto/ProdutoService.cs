using MeuCatalogo.Features.Produto.ApiClients;
using MeuCatalogo.Features.Produto.Local;
using MeuCatalogo.Features.Produto.Requests;
using MeuCatalogo.Features.Produto.Responses;
using MeuCatalogo.Infrastructure;
using Microsoft.Extensions.Logging;
using Refit;

namespace MeuCatalogo.Features.Produto;

public sealed class ProdutoService(
    ILogger<ProdutoService> logger,
    IProdutoApi produtoApi,
    IProdutoLocalRepository produtoLocalRepository) : BaseApiService, IProdutoService
{
    public async Task<ApiResponse<ICollection<ProdutoResponse>>> ObterPorCatalogoIdAsync(Guid catalogoId, CancellationToken ct = default)
    {
        try
        {
            var produtos = await produtoApi.ObterPorCatalogoIdAsync(catalogoId, await ObterBearerTokenAsync(), ct);
            foreach (var produto in produtos)
            {
                NormalizarUrlsImagens(produto);
            }
            await produtoLocalRepository.SaveCatalogoProdutosFromRemoteAsync(catalogoId, produtos, ct);
            return ApiResponse<ICollection<ProdutoResponse>>.Success(produtos);
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao obter produtos.");
            var localProdutos = await produtoLocalRepository.GetProdutosByCatalogoIdAsync(catalogoId, ct);
            if (localProdutos.Count > 0)
                return ApiResponse<ICollection<ProdutoResponse>>.Success(localProdutos);
            return ApiResponse<ICollection<ProdutoResponse>>.Error("Erro ao obter produtos", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao obter produtos.");
            var localProdutos = await produtoLocalRepository.GetProdutosByCatalogoIdAsync(catalogoId, ct);
            if (localProdutos.Count > 0)
                return ApiResponse<ICollection<ProdutoResponse>>.Success(localProdutos);
            return ApiResponse<ICollection<ProdutoResponse>>.Error("Erro inesperado.");
        }
    }

    public async Task<ApiResponse<ProdutoResponse>> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var produto = await produtoApi.ObterPorIdAsync(id, await ObterBearerTokenAsync(), ct);
            NormalizarUrlsImagens(produto);
            await produtoLocalRepository.SaveProdutoFromRemoteAsync(produto, ct);
            return ApiResponse<ProdutoResponse>.Success(produto);
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao obter produto por ID.");
            var localProduto = await produtoLocalRepository.GetProdutoByIdAsync(id, ct);
            if (localProduto != null)
                return ApiResponse<ProdutoResponse>.Success(localProduto);
            return ApiResponse<ProdutoResponse>.Error("Erro ao obter produto", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao obter produto.");
            var localProduto = await produtoLocalRepository.GetProdutoByIdAsync(id, ct);
            if (localProduto != null)
                return ApiResponse<ProdutoResponse>.Success(localProduto);
            return ApiResponse<ProdutoResponse>.Error("Erro inesperado.");
        }
    }

    public async Task<ApiResponse<ProdutoResponse>> CreateAsync(ProdutoCreateRequest request, CancellationToken ct = default)
    {
        try
        {
            var produto = await produtoApi.AdicionarAsync(request, await ObterBearerTokenAsync(), ct);
            NormalizarUrlsImagens(produto);
            produto.SyncStatus = LocalSyncStatus.Synced;
            await produtoLocalRepository.SaveProdutoFromRemoteAsync(produto, ct);
            return ApiResponse<ProdutoResponse>.Success(produto);
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao criar produto.");
            var offlineProduto = BuildOfflineProduto(request, LocalSyncStatus.PendingCreate);
            await produtoLocalRepository.SaveProdutoOfflineAsync(offlineProduto, LocalSyncStatus.PendingCreate, ct);
            return ApiResponse<ProdutoResponse>.Success(offlineProduto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao criar produto.");
            var offlineProduto = BuildOfflineProduto(request, LocalSyncStatus.PendingCreate);
            await produtoLocalRepository.SaveProdutoOfflineAsync(offlineProduto, LocalSyncStatus.PendingCreate, ct);
            return ApiResponse<ProdutoResponse>.Success(offlineProduto);
        }
    }

    public async Task<ApiResponse<ProdutoResponse>> UpdateAsync(Guid id, ProdutoUpdateRequest request, CancellationToken ct = default)
    {
        try
        {
            var produto = await produtoApi.AtualizarAsync(id, request, await ObterBearerTokenAsync(), ct);
            NormalizarUrlsImagens(produto);
            produto.SyncStatus = LocalSyncStatus.Synced;
            await produtoLocalRepository.SaveProdutoFromRemoteAsync(produto, ct);
            return ApiResponse<ProdutoResponse>.Success(produto);
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao atualizar produto.");
            var localProduto = await produtoLocalRepository.GetProdutoByIdAsync(id, ct) ?? new ProdutoResponse
            {
                Id = id,
                CategoriaId = request.CategoriaId
            };

            localProduto.Nome = request.Nome;
            localProduto.Preco = request.Preco;
            localProduto.PrecoComDesconto = request.PrecoComDesconto;
            localProduto.CategoriaId = request.CategoriaId;
            localProduto.InformacoesAdicionais = request.InformacoesAdicionais;
            localProduto.SyncStatus = LocalSyncStatus.PendingUpdate;

            await produtoLocalRepository.SaveProdutoOfflineAsync(localProduto, LocalSyncStatus.PendingUpdate, ct);
            return ApiResponse<ProdutoResponse>.Success(localProduto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao atualizar produto.");
            var localProduto = await produtoLocalRepository.GetProdutoByIdAsync(id, ct) ?? new ProdutoResponse
            {
                Id = id,
                CategoriaId = request.CategoriaId
            };

            localProduto.Nome = request.Nome;
            localProduto.Preco = request.Preco;
            localProduto.PrecoComDesconto = request.PrecoComDesconto;
            localProduto.CategoriaId = request.CategoriaId;
            localProduto.InformacoesAdicionais = request.InformacoesAdicionais;
            localProduto.SyncStatus = LocalSyncStatus.PendingUpdate;

            await produtoLocalRepository.SaveProdutoOfflineAsync(localProduto, LocalSyncStatus.PendingUpdate, ct);
            return ApiResponse<ProdutoResponse>.Success(localProduto);
        }
    }

    public async Task<ApiResponse<Guid>> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            await produtoApi.RemoverAsync(id, await ObterBearerTokenAsync(), ct);
            await produtoLocalRepository.RemoveProdutoAsync(id, ct);
            return ApiResponse<Guid>.Success(id);
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao remover produto.");
            await produtoLocalRepository.MarkProdutoAsDeletedAsync(id, ct);
            return ApiResponse<Guid>.Success(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao remover produto.");
            await produtoLocalRepository.MarkProdutoAsDeletedAsync(id, ct);
            return ApiResponse<Guid>.Success(id);
        }
    }

    public async Task<ApiResponse<ProdutoImagemResponse>> UploadImageAsync(Guid produtoId, FileResult file, CancellationToken ct = default)
    {
        try
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                // Local-first: copy file, create pending record
                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                string localPath = Path.Combine(FileSystem.AppDataDirectory, fileName);
                
                using (var sourceStream = await file.OpenReadAsync())
                using (var destStream = File.Create(localPath))
                {
                    await sourceStream.CopyToAsync(destStream);
                }

                // Check if it's the first image
                var produto = await produtoLocalRepository.GetProdutoByIdAsync(produtoId, ct);
                bool isPrincipal = produto?.Imagens == null || !produto.Imagens.Any();

                var imagem = new ProdutoImagemResponse
                {
                    Id = Guid.NewGuid(),
                    Url = localPath,
                    Images = new ProdutoImagemLinksResponse 
                    { 
                        Thumbnail = localPath, 
                        Medium = localPath, 
                        Full = localPath 
                    },
                    IsPrincipal = isPrincipal,
                    Ordem = (produto?.Imagens?.Count ?? 0) + 1,
                    SyncStatus = LocalSyncStatus.PendingCreate
                };

                await produtoLocalRepository.SaveProdutoImagemOfflineAsync(produtoId, imagem, LocalSyncStatus.PendingCreate, ct);
                return ApiResponse<ProdutoImagemResponse>.Success(imagem);
            }

            using var stream = await file.OpenReadAsync();
            var streamPart = new StreamPart(stream, file.FileName, file.ContentType);
            var result = await produtoApi.UploadImageAsync(produtoId, streamPart, await ObterBearerTokenAsync(), ct);
            result.Url = ObterUrlDisponivel(result);
            result.Images.Thumbnail = NormalizarUrlImagem(result.Images.Thumbnail);
            result.Images.Medium = NormalizarUrlImagem(result.Images.Medium);
            result.Images.Full = NormalizarUrlImagem(result.Images.Full);
            result.SyncStatus = LocalSyncStatus.Synced;
            await produtoLocalRepository.SaveProdutoImagemOfflineAsync(produtoId, result, LocalSyncStatus.Synced, ct);
            return ApiResponse<ProdutoImagemResponse>.Success(result);
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao enviar imagem.");
            var offlineImage = await BuildOfflineImageAsync(produtoId, file);
            await produtoLocalRepository.SaveProdutoImagemOfflineAsync(produtoId, offlineImage, LocalSyncStatus.PendingCreate, ct);
            return ApiResponse<ProdutoImagemResponse>.Success(offlineImage);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao enviar imagem.");
            var offlineImage = await BuildOfflineImageAsync(produtoId, file);
            await produtoLocalRepository.SaveProdutoImagemOfflineAsync(produtoId, offlineImage, LocalSyncStatus.PendingCreate, ct);
            return ApiResponse<ProdutoImagemResponse>.Success(offlineImage);
        }
    }

    private static ProdutoResponse BuildOfflineProduto(ProdutoCreateRequest request, LocalSyncStatus syncStatus)
    {
        return new ProdutoResponse
        {
            Id = Guid.NewGuid(),
            Nome = request.Nome,
            CategoriaId = request.CategoriaId,
            CategoriaNome = "Pendente de sincronização",
            CatalogoId = request.CatalogoId,
            Preco = request.Preco,
            PrecoComDesconto = request.PrecoComDesconto,
            InformacoesAdicionais = request.InformacoesAdicionais,
            Estoque = null,
            Imagens = [],
            SyncStatus = syncStatus
        };
    }

    private async Task<ProdutoImagemResponse> BuildOfflineImageAsync(Guid produtoId, FileResult file)
    {
        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension))
            extension = ".jpg";

        var fileName = $"{produtoId}_{Guid.NewGuid():N}{extension}";
        var localPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

        await using (var source = await file.OpenReadAsync())
        await using (var destination = File.Create(localPath))
        {
            await source.CopyToAsync(destination);
        }

        return new ProdutoImagemResponse
        {
            Id = Guid.NewGuid(),
            Url = localPath,
            Images = new ProdutoImagemLinksResponse
            {
                Thumbnail = localPath,
                Medium = localPath,
                Full = localPath
            },
            IsPrincipal = false,
            Ordem = 999,
            SyncStatus = LocalSyncStatus.PendingCreate
        };
    }

    private static void NormalizarUrlsImagens(ProdutoResponse? produto)
    {
        if (produto?.Imagens == null || produto.Imagens.Count == 0)
            return;

        foreach (var imagem in produto.Imagens)
        {
            imagem.Images.Thumbnail = NormalizarUrlImagem(imagem.Images.Thumbnail);
            imagem.Images.Medium = NormalizarUrlImagem(imagem.Images.Medium);
            imagem.Images.Full = NormalizarUrlImagem(imagem.Images.Full);
            imagem.Url = ObterUrlDisponivel(imagem);
        }
    }

    private static string ObterUrlDisponivel(ProdutoImagemResponse imagem)
    {
        if (!string.IsNullOrWhiteSpace(imagem.Images.Full))
            return imagem.Images.Full;
        if (!string.IsNullOrWhiteSpace(imagem.Url))
            return NormalizarUrlImagem(imagem.Url);
        if (!string.IsNullOrWhiteSpace(imagem.Images.Medium))
            return imagem.Images.Medium;
        if (!string.IsNullOrWhiteSpace(imagem.Images.Thumbnail))
            return imagem.Images.Thumbnail;
        return string.Empty;
    }

    private static string NormalizarUrlImagem(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return string.Empty;

        if (Uri.TryCreate(url, UriKind.Absolute, out var uriAbsoluta))
            return uriAbsoluta.ToString();

        if (!Uri.TryCreate(ApiConstants.BaseUrl, UriKind.Absolute, out var apiBaseUri))
            return url;

        var origemApi = apiBaseUri.GetLeftPart(UriPartial.Authority).TrimEnd('/');
        var caminho = url.StartsWith('/') ? url : $"/{url}";
        return $"{origemApi}{caminho}";
    }
}
