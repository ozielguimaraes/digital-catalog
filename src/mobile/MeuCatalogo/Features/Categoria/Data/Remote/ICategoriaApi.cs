using MeuCatalogo.Features.Categoria.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Categoria.Data.Remote.Contracts.Responses;
using Refit;

namespace MeuCatalogo.Features.Categoria.Data.Remote;

public interface ICategoriaApi
{
    [Get("/categorias/{id}")]
    Task<CategoriaResponse> ObterPorIdAsync(
        Guid id,
        [Header("Authorization")] string bearerToken,
        CancellationToken ct = default);

    [Get("/categorias/catalogo/{catalogoId}")]
    Task<List<CategoriaResponse>> ObterPorCatalogoIdAsync(
        Guid catalogoId,
        [Header("Authorization")] string bearerToken,
        CancellationToken ct = default);

    [Post("/categorias")]
    Task<CategoriaResponse> AdicionarAsync(
        [Body] CategoriaUpsertRequest categoria,
        [Header("Authorization")] string bearerToken,
        CancellationToken ct = default);

    [Put("/categorias/{id}")]
    Task<CategoriaResponse> AtualizarAsync(
        Guid id,
        [Body] CategoriaUpsertRequest categoria,
        [Header("Authorization")] string bearerToken,
        CancellationToken ct = default);
}

