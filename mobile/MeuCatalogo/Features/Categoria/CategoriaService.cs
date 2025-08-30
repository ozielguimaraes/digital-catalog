using MeuCatalogo.Features.Catalogo;
using MeuCatalogo.Features.Categoria.ApiClients;
using MeuCatalogo.Features.Produto.Models;
using Microsoft.Extensions.Logging;
using Refit;

namespace MeuCatalogo.Features.Categoria;

public sealed class CategoriaService(ILogger<CategoriaService> logger, ICategoriaApi categoriaApi) : BaseApiService, ICategoriaService
{
    public async Task<ApiResponse<CategoriaModel>> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var categoria = await categoriaApi.ObterPorIdAsync(id, await ObterBearerTokenAsync(), ct);

            return ApiResponse<CategoriaModel>.Success(categoria);
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao obter categoria por ID.");
            return ApiResponse<CategoriaModel>.Error("Erro ao obter categoria", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao obter categoria para o ID {id}.", id);
            return ApiResponse<CategoriaModel>.Error("Não foi possível carregar a categoria no momento. Tente novamente em instantes.");
        }
    }

    public async Task<ApiResponse<List<CategoriaModel>>> ObterPorCatalogoIdAsync(Guid catalogoId, CancellationToken ct = default)
    {
        try
        {
            var categorias = await categoriaApi.ObterPorCatalogoIdAsync(catalogoId, await ObterBearerTokenAsync(), ct);

            return ApiResponse<List<CategoriaModel>>.Success(categorias);
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao obter categorias para o ID {id}.", catalogoId);
            return ApiResponse<List<CategoriaModel>>.Error("Erro ao obter as categorias", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao obter categorias para o ID {id}.", catalogoId);
            return ApiResponse<List<CategoriaModel>>.Error("Não foi possível carregar as categorias no momento. Tente novamente em instantes.");
        }
    }

    public async Task<ApiResponse<CategoriaModel>> AdicionarAsync(CategoriaModel model, CancellationToken ct = default)
    {
        try
        {
            var categoria = await categoriaApi.AdicionarAsync(model, await ObterBearerTokenAsync(), ct);

            return ApiResponse<CategoriaModel>.Success(categoria);
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao salvar a categoria {nome}.", model.Nome);
            return ApiResponse<CategoriaModel>.Error("Erro ao salvar a categoria", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao salvar a categoria {nome}.", model.Nome);
            return ApiResponse<CategoriaModel>.Error("Não foi possível salvar a categoria no momento. Tente novamente em instantes.");
        }
    }
}
