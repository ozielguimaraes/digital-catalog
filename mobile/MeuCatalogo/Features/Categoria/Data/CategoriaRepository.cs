using MeuCatalogo.Features.Categoria.Data.Remote;
using MeuCatalogo.Features.Categoria.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Categoria.Domain;

namespace MeuCatalogo.Features.Categoria.Data;

public sealed class CategoriaRepository : ICategoriaRepository
{
    private readonly ICategoriaRemoteDataSource _remote;

    public CategoriaRepository(ICategoriaRemoteDataSource remote)
    {
        _remote = remote;
    }

    public Task<ApiResponse<CategoriaInfo>> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _remote.GetByIdAsync(id, ct);

    public Task<ApiResponse<IReadOnlyList<CategoriaInfo>>> GetByCatalogoIdAsync(Guid catalogoId, CancellationToken ct = default)
        => _remote.GetByCatalogoIdAsync(catalogoId, ct);

    public Task<ApiResponse<CategoriaInfo>> CreateAsync(CategoriaUpsertRequest request, CancellationToken ct = default)
        => _remote.CreateAsync(request, ct);

    public Task<ApiResponse<CategoriaInfo>> UpdateAsync(Guid id, CategoriaUpsertRequest request, CancellationToken ct = default)
        => _remote.UpdateAsync(id, request, ct);
}
