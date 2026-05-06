using MeuCatalogo.Features.Auth.Data.Local;
using MeuCatalogo.Features.Categoria.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Categoria.Data.Remote.Contracts.Responses;
using MeuCatalogo.Features.Categoria.Domain;
using Microsoft.Extensions.Logging;
using Refit;

namespace MeuCatalogo.Features.Categoria.Data.Remote;

public sealed class CategoriaRemoteDataSource(
    ILogger<CategoriaRemoteDataSource> logger,
    ICategoriaApi api,
    IAuthLocalDataSource authLocal) : BaseApiService, ICategoriaRemoteDataSource
{
    public async Task<ApiResponse<CategoriaInfo>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var token = await authLocal.GetAccessTokenAsync(ct);
            var categoria = await api.ObterPorIdAsync(id, $"Bearer {token}", ct);
            return ApiResponse<CategoriaInfo>.Success(Map(categoria));
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao obter categoria por ID.");
            return ApiResponse<CategoriaInfo>.Error("Erro ao obter categoria", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao obter categoria para o ID {Id}.", id);
            return ApiResponse<CategoriaInfo>.Error("Não foi possível carregar a categoria no momento. Tente novamente em instantes.");
        }
    }

    public async Task<ApiResponse<IReadOnlyList<CategoriaInfo>>> GetByCatalogoIdAsync(Guid catalogoId, CancellationToken ct = default)
    {
        try
        {
            var token = await authLocal.GetAccessTokenAsync(ct);
            var categorias = await api.ObterPorCatalogoIdAsync(catalogoId, $"Bearer {token}", ct);
            var mapped = categorias.Select(Map).ToList();
            return ApiResponse<IReadOnlyList<CategoriaInfo>>.Success(mapped);
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao obter categorias para o ID {Id}.", catalogoId);
            return ApiResponse<IReadOnlyList<CategoriaInfo>>.Error("Erro ao obter as categorias", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao obter categorias para o ID {Id}.", catalogoId);
            return ApiResponse<IReadOnlyList<CategoriaInfo>>.Error("Não foi possível carregar as categorias no momento. Tente novamente em instantes.");
        }
    }

    public async Task<ApiResponse<CategoriaInfo>> CreateAsync(CategoriaUpsertRequest request, CancellationToken ct = default)
    {
        try
        {
            var token = await authLocal.GetAccessTokenAsync(ct);
            var categoria = await api.AdicionarAsync(request, $"Bearer {token}", ct);
            return ApiResponse<CategoriaInfo>.Success(Map(categoria));
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao salvar a categoria {Nome}.", request.Nome);
            return ApiResponse<CategoriaInfo>.Error("Erro ao salvar a categoria", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao salvar a categoria {Nome}.", request.Nome);
            return ApiResponse<CategoriaInfo>.Error("Não foi possível salvar a categoria no momento. Tente novamente em instantes.");
        }
    }

    public async Task<ApiResponse<CategoriaInfo>> UpdateAsync(Guid id, CategoriaUpsertRequest request, CancellationToken ct = default)
    {
        try
        {
            var token = await authLocal.GetAccessTokenAsync(ct);
            var categoria = await api.AtualizarAsync(id, request, $"Bearer {token}", ct);
            return ApiResponse<CategoriaInfo>.Success(Map(categoria));
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao atualizar a categoria {Nome}.", request.Nome);
            return ApiResponse<CategoriaInfo>.Error("Erro ao atualizar a categoria", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao atualizar a categoria {Nome}.", request.Nome);
            return ApiResponse<CategoriaInfo>.Error("Não foi possível atualizar a categoria no momento. Tente novamente em instantes.");
        }
    }

    private static CategoriaInfo Map(CategoriaResponse c) => new()
    {
        Id = c.Id,
        Nome = c.Nome,
        Descricao = c.Descricao,
        CatalogoId = c.CatalogoId
    };
}
