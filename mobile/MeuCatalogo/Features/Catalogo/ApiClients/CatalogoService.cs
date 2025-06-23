using MeuCatalogo.Features.Catalogo.Requests;
using MeuCatalogo.Features.Catalogo.Responses;
using Microsoft.Extensions.Logging;
using Refit;

namespace MeuCatalogo.Features.Catalogo.ApiClients;

public class CatalogoService(ILogger<CatalogoService> logger, ICatalogoApi catalogoApi) : BaseApiService, ICatalogoService
{
    public async Task<ApiResponse<ICollection<CatalogoResponse>>> GetCatalogosByUserIdAsync(CancellationToken ct = default)
    {
        try
        {
            string? token = await SecureStorage.GetAsync(TokenKey);
            var catalogos = await catalogoApi.ObterCatalogosAsync($"Bearer {token}", ct);
            return ApiResponse<ICollection<CatalogoResponse>>.Success(catalogos);
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao obter catálogos.");
            return ApiResponse<ICollection<CatalogoResponse>>.Error("Erro ao obter catálogos", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao obter catálogos.");
            return ApiResponse<ICollection<CatalogoResponse>>.Error("Erro inesperado.");
        }
    }

    public async Task<ApiResponse<CatalogoResponse>> ObterCatalogoPorIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            string? token = await SecureStorage.GetAsync(TokenKey);
            var catalogo = await catalogoApi.ObterPorIdAsync(id, $"Bearer {token}", ct);
            return ApiResponse<CatalogoResponse>.Success(catalogo);
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao obter catálogo por ID.");
            return ApiResponse<CatalogoResponse>.Error("Erro ao obter catálogo", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao obter catálogo.");
            return ApiResponse<CatalogoResponse>.Error("Erro inesperado.");
        }
    }

    public async Task<ApiResponse<CatalogoResponse>> CreateCatalogoAsync(CatalogoCreateRequest request, CancellationToken ct = default)
    {
        try
        {
            string? token = await SecureStorage.GetAsync(TokenKey);
            var catalogo = await catalogoApi.AdicionarAsync(request, $"Bearer {token}", ct);
            return ApiResponse<CatalogoResponse>.Success(catalogo);
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao criar catálogo.");
            return ApiResponse<CatalogoResponse>.Error("Erro ao criar catálogo", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao criar catálogo.");
            return ApiResponse<CatalogoResponse>.Error("Erro inesperado.");
        }
    }

    public async Task<ApiResponse<CatalogoResponse>> UpdateCatalogoAsync(Guid id, CatalogoUpdateRequest request, CancellationToken ct = default)
    {
        try
        {
            string? token = await SecureStorage.GetAsync(TokenKey);
            var catalogo = await catalogoApi.AtualizarAsync(id, request, $"Bearer {token}", ct);
            return ApiResponse<CatalogoResponse>.Success(catalogo);
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao atualizar catálogo.");
            return ApiResponse<CatalogoResponse>.Error("Erro ao atualizar catálogo", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao atualizar catálogo.");
            return ApiResponse<CatalogoResponse>.Error("Erro inesperado.");
        }
    }

    public async Task<ApiResponse<Guid>> DeleteCatalogoAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            string? token = await SecureStorage.GetAsync(TokenKey);
            await catalogoApi.RemoverAsync(id, $"Bearer {token}", ct);
            return ApiResponse<Guid>.Success(id);
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao remover catálogo.");
            return ApiResponse<Guid>.Error("Erro ao remover catálogo", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao remover catálogo.");
            return ApiResponse<Guid>.Error("Erro inesperado.");
        }
    }
}
