using MeuCatalogo.Features.Pedido.Data;
using MeuCatalogo.Features.Pedido.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Pedido.Domain;

namespace MeuCatalogo.Features.Pedido.UseCases;

public sealed class CreatePedidoUseCase(IPedidoRepository repository)
{
    public Task<ApiResponse<PedidoInfo>> ExecuteAsync(PedidoCreateRequest request, CancellationToken ct = default)
        => repository.CreateAsync(request, ct);
}
