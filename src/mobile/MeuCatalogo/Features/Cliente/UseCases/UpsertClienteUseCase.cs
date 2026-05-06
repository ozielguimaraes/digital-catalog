using MeuCatalogo.Features.Cliente.Data;
using MeuCatalogo.Features.Cliente.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Cliente.Domain;

namespace MeuCatalogo.Features.Cliente.UseCases;

public sealed class UpsertClienteUseCase(IClienteRepository repository)
{
    public Task<ApiResponse<ClienteInfo>> ExecuteAsync(Guid? id, ClienteUpsertRequest request, CancellationToken ct = default)
        => id is null
            ? repository.CreateAsync(request, ct)
            : repository.UpdateAsync(id.Value, request, ct);
}
