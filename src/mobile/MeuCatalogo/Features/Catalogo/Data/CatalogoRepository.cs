using MeuCatalogo.Features.Catalogo.Data.Remote;
using MeuCatalogo.Features.Catalogo.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Catalogo.Domain;

namespace MeuCatalogo.Features.Catalogo.Data;

public sealed class CatalogoRepository(ICatalogoRemoteDataSource remote) : ICatalogoRepository
{
    public Task<ApiResponse<IReadOnlyList<CatalogoInfo>>> GetCatalogosAsync(CancellationToken ct = default)
        => remote.GetCatalogosAsync(ct);

    public Task<ApiResponse<CatalogoInfo>> GetByIdAsync(Guid id, CancellationToken ct = default)
        => remote.GetByIdAsync(id, ct);

    public Task<ApiResponse<CatalogoInfo>> CreateAsync(CatalogoCreateRequest request, CancellationToken ct = default)
        => remote.CreateAsync(request, ct);

    public Task<ApiResponse<CatalogoInfo>> UpdateAsync(Guid id, CatalogoUpdateRequest request, CancellationToken ct = default)
        => remote.UpdateAsync(id, request, ct);

    public Task<ApiResponse<Guid>> DeleteAsync(Guid id, CancellationToken ct = default)
        => remote.DeleteAsync(id, ct);
}
