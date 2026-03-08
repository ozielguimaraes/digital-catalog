using MeuCatalogo.Features.Produto.ApiClients;
using MeuCatalogo.Features.Produto.Requests;
using MeuCatalogo.Features.Produto.Responses;
using MeuCatalogo.Infrastructure;
using Microsoft.Extensions.Logging;
using Refit;

namespace MeuCatalogo.Features.Produto;

public sealed class ProdutoService(ILogger<ProdutoService> logger, IProdutoApi produtoApi) : BaseApiService, IProdutoService
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
            var produto = await produtoApi.ObterPorIdAsync(id, await ObterBearerTokenAsync(), ct);
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
            var produto = await produtoApi.AdicionarAsync(request, await ObterBearerTokenAsync(), ct);
            NormalizarUrlsImagens(produto);
            return ApiResponse<ProdutoResponse>.Success(produto);
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao criar produto.");
            return ApiResponse<ProdutoResponse>.Error("Erro ao criar produto", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao criar produto.");
            return ApiResponse<ProdutoResponse>.Error("Erro inesperado.");
        }
    }

    public async Task<ApiResponse<ProdutoResponse>> UpdateAsync(Guid id, ProdutoUpdateRequest request, CancellationToken ct = default)
    {
        try
        {
            var produto = await produtoApi.AtualizarAsync(id, request, await ObterBearerTokenAsync(), ct);
            NormalizarUrlsImagens(produto);
            return ApiResponse<ProdutoResponse>.Success(produto);
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao atualizar produto.");
            return ApiResponse<ProdutoResponse>.Error("Erro ao atualizar produto", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao atualizar produto.");
            return ApiResponse<ProdutoResponse>.Error("Erro inesperado.");
        }
    }

    public async Task<ApiResponse<Guid>> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            await produtoApi.RemoverAsync(id, await ObterBearerTokenAsync(), ct);
            return ApiResponse<Guid>.Success(id);
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao remover produto.");
            return ApiResponse<Guid>.Error("Erro ao remover produto", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao remover produto.");
            return ApiResponse<Guid>.Error("Erro inesperado.");
        }
    }

    public async Task<ApiResponse<ProdutoImagemResponse>> UploadImageAsync(Guid produtoId, FileResult file, CancellationToken ct = default)
    {
        try
        {
            using var stream = await file.OpenReadAsync();
            var streamPart = new StreamPart(stream, file.FileName, file.ContentType);
            var result = await produtoApi.UploadImageAsync(produtoId, streamPart, await ObterBearerTokenAsync(), ct);
            result.Url = ObterUrlDisponivel(result);
            result.Images.Thumbnail = NormalizarUrlImagem(result.Images.Thumbnail);
            result.Images.Medium = NormalizarUrlImagem(result.Images.Medium);
            result.Images.Full = NormalizarUrlImagem(result.Images.Full);
            return ApiResponse<ProdutoImagemResponse>.Success(result);
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao enviar imagem.");
            return ApiResponse<ProdutoImagemResponse>.Error("Erro ao enviar imagem", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao enviar imagem.");
            return ApiResponse<ProdutoImagemResponse>.Error("Erro inesperado.");
        }
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
