using MeuCatalogo.Features.Pedido.Data.Remote;
using MeuCatalogo.Features.Pedido.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Pedido.Domain;

namespace MeuCatalogo.Features.Pedido.Data;

public sealed class PedidoRepository(IPedidoRemoteDataSource remote) : IPedidoRepository
{
    public Task<ApiResponse<IReadOnlyList<PedidoInfo>>> GetAllAsync(CancellationToken ct = default)
        => remote.GetAllAsync(ct);

    public Task<ApiResponse<PedidoInfo>> GetByIdAsync(Guid id, CancellationToken ct = default)
        => remote.GetByIdAsync(id, ct);

    public Task<ApiResponse<IReadOnlyList<PedidoInfo>>> GetByClienteAsync(Guid clienteId, CancellationToken ct = default)
        => remote.GetByClienteAsync(clienteId, ct);

    public Task<ApiResponse<PedidoInfo>> CreateAsync(PedidoCreateRequest request, CancellationToken ct = default)
        => remote.CreateAsync(request, ct);

    public Task<ApiResponse<Guid>> DeleteAsync(Guid id, CancellationToken ct = default)
        => remote.DeleteAsync(id, ct);
}
