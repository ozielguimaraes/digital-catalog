using MeuCatalogo.Features.Catalogo.Data.Remote;
using MeuCatalogo.Features.Catalogo.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Catalogo.Domain;

namespace MeuCatalogo.Features.Catalogo.Data;

public sealed class CatalogoRepository : ICatalogoRepository
{
    private readonly ICatalogoRemoteDataSource _remote;

    public CatalogoRepository(ICatalogoRemoteDataSource remote)
    {
        _remote = remote;
    }

    public Task<ApiResponse<IReadOnlyList<CatalogoInfo>>> GetCatalogosAsync(CancellationToken ct = default)
        => _remote.GetCatalogosAsync(ct);

    public Task<ApiResponse<CatalogoInfo>> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _remote.GetByIdAsync(id, ct);

    public Task<ApiResponse<CatalogoInfo>> CreateAsync(CatalogoCreateRequest request, CancellationToken ct = default)
        => _remote.CreateAsync(request, ct);

    public Task<ApiResponse<CatalogoInfo>> UpdateAsync(Guid id, CatalogoUpdateRequest request, CancellationToken ct = default)
        => _remote.UpdateAsync(id, request, ct);

    public Task<ApiResponse<Guid>> DeleteAsync(Guid id, CancellationToken ct = default)
        => _remote.DeleteAsync(id, ct);
}
