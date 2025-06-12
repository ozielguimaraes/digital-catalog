using MeuCatalogo.Features.Catalogo.Requests;
using MeuCatalogo.Features.Catalogo.Responses;

namespace MeuCatalogo.Features.Catalogo.ApiClients;

public interface ICatalogoService
{
    Task<ApiResponse<ICollection<CatalogoResponse>>> GetCatalogosByUserIdAsync(string userId, CancellationToken ct = default);
    Task<ApiResponse<CatalogoResponse>> ObterCatalogoPorIdAsync(Guid id, string userId, CancellationToken ct = default);
    Task<ApiResponse<CatalogoResponse>> CreateCatalogoAsync(CatalogoCreateRequest request, string userId, CancellationToken ct = default);
    Task<ApiResponse<CatalogoResponse>> UpdateCatalogoAsync(Guid id, CatalogoUpdateRequest request, string userId, CancellationToken ct = default);
    Task<ApiResponse<Guid>> DeleteCatalogoAsync(Guid id, string userId, CancellationToken ct = default);
}
