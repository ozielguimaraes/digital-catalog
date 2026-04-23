using MeuCatalogo.Domain.Enums;
using MeuCatalogo.Features.Auth.Data.Local;
using MeuCatalogo.Features.Produto.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Produto.Data.Remote.Contracts.Responses;
using Microsoft.Extensions.Logging;
using Refit;

namespace MeuCatalogo.Features.Produto.Data.Remote;

public sealed class ProdutoRemoteDataSource(
    ILogger<ProdutoRemoteDataSource> logger,
    IProdutoApi produtoApi,
    IAuthLocalDataSource authLocal) : BaseApiService, IProdutoRemoteDataSource
{
    public async Task<ApiResponse<ICollection<ProdutoResponse>>> ObterPorCatalogoIdAsync(Guid catalogoId, CancellationToken ct = default)
    {
        try
        {
            var token = await authLocal.GetAccessTokenAsync(ct);
            var produtos = await produtoApi.ObterPorCatalogoIdAsync(catalogoId, $"Bearer {token}", ct);
            foreach (var produto in produtos)
            {
                NormalizarUrlsImagens(produto);
            }
            return ApiResponse<ICollection<ProdutoResponse>>.Success(produtos);
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao obter produtos.");
            return ApiResponse<ICollection<ProdutoResponse>>.Error("Erro ao obter produtos", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao obter produtos.");
            return ApiResponse<ICollection<ProdutoResponse>>.Error("Erro inesperado.");
        }
    }

    public async Task<ApiResponse<ProdutoResponse>> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var token = await authLocal.GetAccessTokenAsync(ct);
            var produto = await produtoApi.ObterPorIdAsync(id, $"Bearer {token}", ct);
            NormalizarUrlsImagens(produto);
            return ApiResponse<ProdutoResponse>.Success(produto);
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao obter produto por ID.");
            return ApiResponse<ProdutoResponse>.Error("Erro ao obter produto", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao obter produto.");
            return ApiResponse<ProdutoResponse>.Error("Erro inesperado.");
        }
    }

    public async Task<ApiResponse<ProdutoResponse>> CreateAsync(ProdutoCreateRequest request, CancellationToken ct = default)
    {
        try
        {
            var token = await authLocal.GetAccessTokenAsync(ct);
            var produto = await produtoApi.AdicionarAsync(request, $"Bearer {token}", ct);
            NormalizarUrlsImagens(produto);
            produto.SyncStatus = SyncStatus.Completed;
            return ApiResponse<ProdutoResponse>.Success(produto);
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao criar produto.");
            var offlineProduto = BuildOfflineProduto(request, SyncStatus.Pending);
            return ApiResponse<ProdutoResponse>.Success(offlineProduto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao criar produto.");
            var offlineProduto = BuildOfflineProduto(request, SyncStatus.Pending);
            return ApiResponse<ProdutoResponse>.Success(offlineProduto);
        }
    }

    public async Task<ApiResponse<ProdutoResponse>> UpdateAsync(Guid id, ProdutoUpdateRequest request, CancellationToken ct = default)
    {
        try
        {
            var token = await authLocal.GetAccessTokenAsync(ct);
            var produto = await produtoApi.AtualizarAsync(id, request, $"Bearer {token}", ct);
            NormalizarUrlsImagens(produto);
            produto.SyncStatus = SyncStatus.Completed;
            return ApiResponse<ProdutoResponse>.Success(produto);
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao atualizar produto.");
            return ApiResponse<ProdutoResponse>.Error("Erro ao atualizar produto.", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao atualizar produto.");
            return ApiResponse<ProdutoResponse>.Error("Erro inesperado ao atualizar produto.");
        }
    }

    public async Task<ApiResponse<Guid>> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var token = await authLocal.GetAccessTokenAsync(ct);
            await produtoApi.RemoverAsync(id, $"Bearer {token}", ct);
            return ApiResponse<Guid>.Success(id);
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao remover produto.");
            return ApiResponse<Guid>.Error("Erro ao remover produto.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao remover produto.");
            return ApiResponse<Guid>.Error("Erro inesperado ao remover produto.");
        }
    }

    public async Task<ApiResponse<ProdutoImagemResponse>> UploadImageAsync(Guid produtoId, FileResult file, bool isPrincipal = false, int ordem = 0, CancellationToken ct = default)
    {
        try
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                string localPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

                using (var sourceStream = await file.OpenReadAsync())
                using (var destStream = File.Create(localPath))
                {
                    await sourceStream.CopyToAsync(destStream);
                }

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
                    Ordem = ordem == 0 ? 1 : ordem,
                    SyncStatus = SyncStatus.Pending
                };

                return ApiResponse<ProdutoImagemResponse>.Success(imagem);
            }

            using var stream = await file.OpenReadAsync();
            var streamPart = new StreamPart(stream, file.FileName, file.ContentType);
            var token = await authLocal.GetAccessTokenAsync(ct);
            var result = await produtoApi.UploadImageAsync(produtoId, streamPart, $"Bearer {token}", isPrincipal, ordem, ct);
            result.Url = ObterUrlDisponivel(result);
            result.Images.Thumbnail = NormalizarUrlImagem(result.Images.Thumbnail);
            result.Images.Medium = NormalizarUrlImagem(result.Images.Medium);
            result.Images.Full = NormalizarUrlImagem(result.Images.Full);
            result.SyncStatus = SyncStatus.Completed;
            return ApiResponse<ProdutoImagemResponse>.Success(result);
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao enviar imagem.");
            var offlineImage = await BuildOfflineImageAsync(produtoId, file, isPrincipal, ordem);
            return ApiResponse<ProdutoImagemResponse>.Success(offlineImage);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao enviar imagem.");
            var offlineImage = await BuildOfflineImageAsync(produtoId, file, isPrincipal, ordem);
            return ApiResponse<ProdutoImagemResponse>.Success(offlineImage);
        }
    }

    private static ProdutoResponse BuildOfflineProduto(ProdutoCreateRequest request, SyncStatus syncStatus)
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

    private async Task<ProdutoImagemResponse> BuildOfflineImageAsync(Guid produtoId, FileResult file, bool isPrincipal, int ordem)
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
            IsPrincipal = isPrincipal,
            Ordem = ordem,
            SyncStatus = SyncStatus.Pending
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

        if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase) || url.StartsWith("file://", StringComparison.OrdinalIgnoreCase) || File.Exists(url))
            return url;

        return url.TrimStart('/');
    }
}

