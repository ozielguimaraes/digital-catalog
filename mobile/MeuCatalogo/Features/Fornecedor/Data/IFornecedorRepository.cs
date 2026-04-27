using MeuCatalogo.Features.Fornecedor.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Fornecedor.Domain;

namespace MeuCatalogo.Features.Fornecedor.Data;

public interface IFornecedorRepository
{
    Task<ApiResponse<IReadOnlyList<FornecedorInfo>>> GetAllAsync(CancellationToken ct = default);
    Task<ApiResponse<FornecedorInfo>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<FornecedorInfo>> CreateAsync(FornecedorUpsertRequest request, CancellationToken ct = default);
    Task<ApiResponse<FornecedorInfo>> UpdateAsync(Guid id, FornecedorUpsertRequest request, CancellationToken ct = default);
    Task<ApiResponse<Guid>> DeleteAsync(Guid id, CancellationToken ct = default);
}
