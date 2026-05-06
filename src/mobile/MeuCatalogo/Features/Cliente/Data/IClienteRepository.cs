using MeuCatalogo.Features.Cliente.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Cliente.Domain;

namespace MeuCatalogo.Features.Cliente.Data;

public interface IClienteRepository
{
    Task<ApiResponse<IReadOnlyList<ClienteInfo>>> GetAllAsync(CancellationToken ct = default);
    Task<ApiResponse<ClienteInfo>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<ClienteInfo>> CreateAsync(ClienteUpsertRequest request, CancellationToken ct = default);
    Task<ApiResponse<ClienteInfo>> UpdateAsync(Guid id, ClienteUpsertRequest request, CancellationToken ct = default);
    Task<ApiResponse<Guid>> DeleteAsync(Guid id, CancellationToken ct = default);
}
