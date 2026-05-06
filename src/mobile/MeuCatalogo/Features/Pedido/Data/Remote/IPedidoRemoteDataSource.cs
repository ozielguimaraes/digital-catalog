using MeuCatalogo.Features.Pedido.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Pedido.Domain;

namespace MeuCatalogo.Features.Pedido.Data.Remote;

public interface IPedidoRemoteDataSource
{
    Task<ApiResponse<IReadOnlyList<PedidoInfo>>> GetAllAsync(CancellationToken ct = default);
    Task<ApiResponse<PedidoInfo>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<IReadOnlyList<PedidoInfo>>> GetByClienteAsync(Guid clienteId, CancellationToken ct = default);
    Task<ApiResponse<PedidoInfo>> CreateAsync(PedidoCreateRequest request, CancellationToken ct = default);
    Task<ApiResponse<Guid>> DeleteAsync(Guid id, CancellationToken ct = default);
}
