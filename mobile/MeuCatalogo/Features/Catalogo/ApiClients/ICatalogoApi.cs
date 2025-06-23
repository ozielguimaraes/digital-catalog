using MeuCatalogo.Features.Catalogo.Requests;
using MeuCatalogo.Features.Catalogo.Responses;
using Refit;

namespace MeuCatalogo.Features.Catalogo.ApiClients;

public interface ICatalogoApi
{
    [Get("/catalogos/")]
    Task<ICollection<CatalogoResponse>> ObterCatalogosAsync([Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Get("/catalogos/{id}")]
    Task<CatalogoResponse> ObterPorIdAsync(Guid id, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Post("/catalogos")]
    Task<CatalogoResponse> AdicionarAsync([Body] CatalogoCreateRequest request, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Put("/catalogos/{id}")]
    Task<CatalogoResponse> AtualizarAsync(Guid id, [Body] CatalogoUpdateRequest request, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Delete("/catalogos/{id}")]
    Task RemoverAsync(Guid id, [Header("Authorization")] string bearerToken, CancellationToken ct = default);
}
