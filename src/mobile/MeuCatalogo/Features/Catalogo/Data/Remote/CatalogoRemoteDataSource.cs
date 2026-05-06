using MeuCatalogo.Features.Auth.Data.Local;
using MeuCatalogo.Features.Catalogo.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Catalogo.Data.Remote.Contracts.Responses;
using MeuCatalogo.Features.Catalogo.Domain;
using Microsoft.Extensions.Logging;
using Refit;

namespace MeuCatalogo.Features.Catalogo.Data.Remote;

public sealed class CatalogoRemoteDataSource(
    ILogger<CatalogoRemoteDataSource> logger,
    ICatalogoApi api,
    IAuthLocalDataSource authLocal) : BaseApiService, ICatalogoRemoteDataSource
{
    public async Task<ApiResponse<IReadOnlyList<CatalogoInfo>>> GetCatalogosAsync(CancellationToken ct = default)
    {
        try
        {
            var token = await authLocal.GetAccessTokenAsync(ct);
            var catalogos = await api.ObterCatalogosAsync($"Bearer {token}", ct);
            var mapped = catalogos.Select(Map).ToList();
            return ApiResponse<IReadOnlyList<CatalogoInfo>>.Success(mapped);
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao obter catálogos.");
            return ApiResponse<IReadOnlyList<CatalogoInfo>>.Error("Erro ao obter catálogos", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao obter catálogos.");
            return ApiResponse<IReadOnlyList<CatalogoInfo>>.Error("Erro inesperado.");
        }
    }

    public async Task<ApiResponse<CatalogoInfo>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var token = await authLocal.GetAccessTokenAsync(ct);
            var catalogo = await api.ObterPorIdAsync(id, $"Bearer {token}", ct);
            return ApiResponse<CatalogoInfo>.Success(Map(catalogo));
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao obter catálogo por ID.");
            return ApiResponse<CatalogoInfo>.Error("Erro ao obter catálogo", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao obter catálogo.");
            return ApiResponse<CatalogoInfo>.Error("Erro inesperado.");
        }
    }

    public async Task<ApiResponse<CatalogoInfo>> CreateAsync(CatalogoCreateRequest request, CancellationToken ct = default)
    {
        try
        {
            var token = await authLocal.GetAccessTokenAsync(ct);
            var created = await api.AdicionarAsync(request, $"Bearer {token}", ct);
            return ApiResponse<CatalogoInfo>.Success(Map(created));
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao criar catálogo.");
            return ApiResponse<CatalogoInfo>.Error("Erro ao criar catálogo", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao criar catálogo.");
            return ApiResponse<CatalogoInfo>.Error("Erro inesperado.");
        }
    }

    public async Task<ApiResponse<CatalogoInfo>> UpdateAsync(Guid id, CatalogoUpdateRequest request, CancellationToken ct = default)
    {
        try
        {
            var token = await authLocal.GetAccessTokenAsync(ct);
            var updated = await api.AtualizarAsync(id, request, $"Bearer {token}", ct);
            return ApiResponse<CatalogoInfo>.Success(Map(updated));
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao atualizar catálogo.");
            return ApiResponse<CatalogoInfo>.Error("Erro ao atualizar catálogo", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao atualizar catálogo.");
            return ApiResponse<CatalogoInfo>.Error("Erro inesperado.");
        }
    }

    public async Task<ApiResponse<Guid>> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var token = await authLocal.GetAccessTokenAsync(ct);
            await api.RemoverAsync(id, $"Bearer {token}", ct);
            return ApiResponse<Guid>.Success(id);
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao remover catálogo {Id}.", id);
            return ApiResponse<Guid>.Error("Não conseguimos concluir a remoção. Se o problema persistir, entre em contato com o suporte", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao remover catálogo.");
            return ApiResponse<Guid>.Error("Não conseguimos remover o catálogo agora. Verifique sua conexão e tente novamente.");
        }
    }

    private static CatalogoInfo Map(CatalogoResponse c) => new()
    {
        Id = c.Id,
        Nome = c.Nome,
        NomeCurto = c.NomeCurto,
        Email = c.Email,
        NumeroWhatsapp = c.NumeroWhatsapp,
        Descricao = c.Descricao
    };
}
