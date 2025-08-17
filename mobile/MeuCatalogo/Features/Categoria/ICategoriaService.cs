using MeuCatalogo.Features.Produto.Models;

namespace MeuCatalogo.Features.Categoria;

public interface ICategoriaService
{
    Task<ApiResponse<CategoriaModel>> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<List<CategoriaModel>>> ObterPorCatalogoIdAsync(Guid catalogoId, CancellationToken ct = default);
}
