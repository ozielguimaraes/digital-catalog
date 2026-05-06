using MeuCatalogo.Features.Cliente.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Cliente.Data.Remote.Contracts.Responses;
using Refit;

namespace MeuCatalogo.Features.Cliente.Data.Remote;

public interface IClienteApi
{
    [Get("/clientes")]
    Task<ICollection<ClienteResponse>> ObterTodosAsync([Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Get("/clientes/{id}")]
    Task<ClienteResponse> ObterPorIdAsync(Guid id, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Post("/clientes")]
    Task<ClienteResponse> CriarAsync([Body] ClienteUpsertRequest request, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Put("/clientes/{id}")]
    Task<ClienteResponse> AtualizarAsync(Guid id, [Body] ClienteUpsertRequest request, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Delete("/clientes/{id}")]
    Task RemoverAsync(Guid id, [Header("Authorization")] string bearerToken, CancellationToken ct = default);
}
