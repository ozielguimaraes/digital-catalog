using MeuCatalogo.Features.Fornecedor.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Fornecedor.Data.Remote.Contracts.Responses;
using Refit;

namespace MeuCatalogo.Features.Fornecedor.Data.Remote;

public interface IFornecedorApi
{
    [Get("/fornecedores")]
    Task<ICollection<FornecedorResponse>> ObterTodosAsync([Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Get("/fornecedores/{id}")]
    Task<FornecedorResponse> ObterPorIdAsync(Guid id, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Post("/fornecedores")]
    Task<FornecedorResponse> CriarAsync([Body] FornecedorUpsertRequest request, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Put("/fornecedores/{id}")]
    Task<FornecedorResponse> AtualizarAsync(Guid id, [Body] FornecedorUpsertRequest request, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Delete("/fornecedores/{id}")]
    Task RemoverAsync(Guid id, [Header("Authorization")] string bearerToken, CancellationToken ct = default);
}
