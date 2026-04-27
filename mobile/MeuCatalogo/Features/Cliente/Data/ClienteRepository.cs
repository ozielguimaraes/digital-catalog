using MeuCatalogo.Features.Cliente.Data.Remote;
using MeuCatalogo.Features.Cliente.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Cliente.Domain;

namespace MeuCatalogo.Features.Cliente.Data;

public sealed class ClienteRepository(IClienteRemoteDataSource remote) : IClienteRepository
{
    public Task<ApiResponse<IReadOnlyList<ClienteInfo>>> GetAllAsync(CancellationToken ct = default)
        => remote.GetAllAsync(ct);

    public Task<ApiResponse<ClienteInfo>> GetByIdAsync(Guid id, CancellationToken ct = default)
        => remote.GetByIdAsync(id, ct);

    public Task<ApiResponse<ClienteInfo>> CreateAsync(ClienteUpsertRequest request, CancellationToken ct = default)
        => remote.CreateAsync(request, ct);

    public Task<ApiResponse<ClienteInfo>> UpdateAsync(Guid id, ClienteUpsertRequest request, CancellationToken ct = default)
        => remote.UpdateAsync(id, request, ct);

    public Task<ApiResponse<Guid>> DeleteAsync(Guid id, CancellationToken ct = default)
        => remote.DeleteAsync(id, ct);
}
