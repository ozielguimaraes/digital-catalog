using MeuCatalogo.Features.Cliente.Data;

namespace MeuCatalogo.Features.Cliente.UseCases;

public sealed class DeleteClienteUseCase(IClienteRepository repository)
{
    public Task<ApiResponse<Guid>> ExecuteAsync(Guid id, CancellationToken ct = default)
        => repository.DeleteAsync(id, ct);
}
