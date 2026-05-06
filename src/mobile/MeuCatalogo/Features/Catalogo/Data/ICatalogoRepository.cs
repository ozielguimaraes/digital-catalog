using MeuCatalogo.Features.Catalogo.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Catalogo.Domain;

namespace MeuCatalogo.Features.Catalogo.Data;

public interface ICatalogoRepository
{
    Task<ApiResponse<IReadOnlyList<CatalogoInfo>>> GetCatalogosAsync(CancellationToken ct = default);
    Task<ApiResponse<CatalogoInfo>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<CatalogoInfo>> CreateAsync(CatalogoCreateRequest request, CancellationToken ct = default);
    Task<ApiResponse<CatalogoInfo>> UpdateAsync(Guid id, CatalogoUpdateRequest request, CancellationToken ct = default);
    Task<ApiResponse<Guid>> DeleteAsync(Guid id, CancellationToken ct = default);
}
