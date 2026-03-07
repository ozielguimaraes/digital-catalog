using MeuCatalogo.Features.Produto.Requests;
using MeuCatalogo.Features.Produto.Responses;
using Refit;
using System.Net.Http;
using System.Threading;

namespace MeuCatalogo.Features.Produto.ApiClients;

public interface IProdutoApi
{
    [Get("/produtos/catalogo/{catalogoId}")]
    Task<ICollection<ProdutoResponse>> ObterPorCatalogoIdAsync(
        Guid catalogoId,
        [Header("Authorization")] string bearerToken,
        CancellationToken ct = default);

    [Get("/produtos/{id}")]
    Task<ProdutoResponse> ObterPorIdAsync(
        Guid id,
        [Header("Authorization")] string bearerToken,
        CancellationToken ct = default);

    [Post("/produtos")]
    Task<ProdutoResponse> AdicionarAsync(
        [Body] ProdutoCreateRequest request,
        [Header("Authorization")] string bearerToken,
        CancellationToken ct = default);

    [Put("/produtos/{id}")]
    Task<ProdutoResponse> AtualizarAsync(
        Guid id,
        [Body] ProdutoUpdateRequest request,
        [Header("Authorization")] string bearerToken,
        CancellationToken ct = default);

    [Delete("/produtos/{id}")]
    Task RemoverAsync(
        Guid id,
        [Header("Authorization")] string bearerToken,
        CancellationToken ct = default);

    [Multipart]
    [Post("/produtos/{produtoId}/upload-image")]
    Task<ProdutoImagemResponse> UploadImageAsync(
        Guid produtoId,
        [AliasAs("file")] StreamPart file,
        [Header("Authorization")] string bearerToken,
        CancellationToken ct = default);
}
