using MeuCatalogo.Features.Pedido.Data;

namespace MeuCatalogo.Features.Pedido.UseCases;

public sealed class DeletePedidoUseCase(IPedidoRepository repository)
{
    public async Task<ApiResponse<Guid>> ExecuteAsync(Guid id, CancellationToken ct = default)
        => await repository.DeleteAsync(id, ct);
}
