using MeuCatalogo.Features.Cliente.Data;
using MeuCatalogo.Features.Cliente.Domain;

namespace MeuCatalogo.Features.Cliente.UseCases;

public sealed class GetClientesUseCase(IClienteRepository repository)
{
    public async Task<ApiResponse<IReadOnlyList<ClienteInfo>>> ExecuteAsync(CancellationToken ct = default)
        => await repository.GetAllAsync(ct);
}
