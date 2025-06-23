using MeuCatalogo.Features.Produto.ApiClients;
using MeuCatalogo.Features.Produto.Requests;
using MeuCatalogo.Features.Produto.Responses;
using Microsoft.Extensions.Logging;
using Refit;

namespace MeuCatalogo.Features.Produto;

public sealed class ProdutoService(ILogger<ProdutoService> logger, IProdutoApi produtoApi) : BaseApiService, IProdutoService
{
    public async Task<ApiResponse<ICollection<ProdutoResponse>>> ObterPorCatalogoIdAsync(Guid catalogoId, CancellationToken ct = default)
    {
        try
        {
            string? token = await SecureStorage.GetAsync(TokenKey);
            var produtos = await produtoApi.ObterPorCatalogoIdAsync(catalogoId, $"Bearer {token}", ct);
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
            string? token = await SecureStorage.GetAsync(TokenKey);
            var produto = await produtoApi.ObterPorIdAsync(id, $"Bearer {token}", ct);
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
            string? token = await SecureStorage.GetAsync(TokenKey);
            var produto = await produtoApi.AdicionarAsync(request, $"Bearer {token}", ct);
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
            string? token = await SecureStorage.GetAsync(TokenKey);
            var produto = await produtoApi.AtualizarAsync(id, request, $"Bearer {token}", ct);
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
            string? token = await SecureStorage.GetAsync(TokenKey);
            await produtoApi.RemoverAsync(id, $"Bearer {token}", ct);
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
}
