using MeuCatalogo.Features.Produto.Models;
using Refit;

namespace MeuCatalogo.Features.Categoria.ApiClients;

public interface ICategoriaApi
{
    [Get("/categorias/{id}")]
    Task<CategoriaModel> ObterPorIdAsync(
        Guid id,
        [Header("Authorization")] string bearerToken,
        CancellationToken ct = default);

    [Get("/categorias/catalogo/{catalogoId}")]
    Task<List<CategoriaModel>> ObterPorCatalogoIdAsync(
        Guid catalogoId,
        [Header("Authorization")] string bearerToken,
        CancellationToken ct = default);

    [Post("/categorias")]
    Task<CategoriaModel> AdicionarAsync(
        [Body] CategoriaModel categoria,
        [Header("Authorization")] string bearerToken,
        CancellationToken ct = default);
}
