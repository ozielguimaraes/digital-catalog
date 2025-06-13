using MeuCatalogo.Features.Catalogo.Requests;
using MeuCatalogo.Features.Catalogo.Responses;

namespace MeuCatalogo.Features.Catalogo.ApiClients;

public interface ICatalogoService
{
    Task<ApiResponse<ICollection<CatalogoResponse>>> GetCatalogosByUserIdAsync(CancellationToken ct = default);
    Task<ApiResponse<CatalogoResponse>> ObterCatalogoPorIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<CatalogoResponse>> CreateCatalogoAsync(CatalogoCreateRequest request, CancellationToken ct = default);
    Task<ApiResponse<CatalogoResponse>> UpdateCatalogoAsync(Guid id, CatalogoUpdateRequest request, CancellationToken ct = default);
    Task<ApiResponse<Guid>> DeleteCatalogoAsync(Guid id, CancellationToken ct = default);
}
