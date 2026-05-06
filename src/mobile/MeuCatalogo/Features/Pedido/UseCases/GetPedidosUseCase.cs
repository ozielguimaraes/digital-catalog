using MeuCatalogo.Features.Pedido.Data;
using MeuCatalogo.Features.Pedido.Domain;

namespace MeuCatalogo.Features.Pedido.UseCases;

public sealed class GetPedidosUseCase(IPedidoRepository repository)
{
    public async Task<ApiResponse<IReadOnlyList<PedidoInfo>>> ExecuteAsync(CancellationToken ct = default)
        => await repository.GetAllAsync(ct);
}
