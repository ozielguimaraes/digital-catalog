using MeuCatalogo.Features.Categoria.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Categoria.Domain;

namespace MeuCatalogo.Features.Categoria.Data;

public interface ICategoriaRepository
{
    Task<ApiResponse<CategoriaInfo>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<IReadOnlyList<CategoriaInfo>>> GetByCatalogoIdAsync(Guid catalogoId, CancellationToken ct = default);
    Task<ApiResponse<CategoriaInfo>> CreateAsync(CategoriaUpsertRequest request, CancellationToken ct = default);
    Task<ApiResponse<CategoriaInfo>> UpdateAsync(Guid id, CategoriaUpsertRequest request, CancellationToken ct = default);
}
