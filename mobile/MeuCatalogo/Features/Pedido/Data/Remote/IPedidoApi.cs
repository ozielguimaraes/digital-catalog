using MeuCatalogo.Features.Pedido.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Pedido.Data.Remote.Contracts.Responses;
using Refit;

namespace MeuCatalogo.Features.Pedido.Data.Remote;

public interface IPedidoApi
{
    [Get("/pedidos")]
    Task<ICollection<PedidoResponse>> ObterTodosAsync([Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Get("/pedidos/{id}")]
    Task<PedidoResponse> ObterPorIdAsync(Guid id, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Get("/pedidos/cliente/{clienteId}")]
    Task<ICollection<PedidoResponse>> ObterPorClienteAsync(Guid clienteId, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Post("/pedidos")]
    Task<PedidoResponse> CriarAsync([Body] PedidoCreateRequest request, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Delete("/pedidos/{id}")]
    Task RemoverAsync(Guid id, [Header("Authorization")] string bearerToken, CancellationToken ct = default);
}
