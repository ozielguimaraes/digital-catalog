using MeuCatalogo.Application.DTOs.Categoria;
using MeuCatalogo.Application.DTOs.Responses;

namespace MeuCatalogo.Application.Interfaces;

public interface ICategoriaService
{
    Task<ApiResponse<CategoriaResponse>> AdicionarAsync(string usuarioId, CategoriaRequest request);
    Task<ApiResponse<CategoriaResponse>> ObterPorIdAsync(string usuarioId, Guid categoriaId);
    Task<ApiResponse<IList<CategoriaResponse>>> ObterPorCatalogoAsync(Guid catalogoId, string usuarioId);
    Task<ApiResponse<CategoriaResponse>> AtualizarAsync(Guid id, string usuarioId, AtualizarCategoriaRequest request);
    Task<ApiResponse<bool>> RemoverAsync(Guid id, string usuarioId);
}
