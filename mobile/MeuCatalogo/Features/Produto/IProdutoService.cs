using MeuCatalogo.Features.Produto.Requests;
using MeuCatalogo.Features.Produto.Responses;

namespace MeuCatalogo.Features.Produto;

public interface IProdutoService
{
    Task<ApiResponse<ICollection<ProdutoResponse>>> ObterPorCatalogoIdAsync(Guid catalogoId, CancellationToken ct = default);
    Task<ApiResponse<ProdutoResponse>> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<ProdutoResponse>> CreateAsync(ProdutoCreateRequest request, CancellationToken ct = default);
    Task<ApiResponse<ProdutoResponse>> UpdateAsync(Guid id, ProdutoUpdateRequest request, CancellationToken ct = default);
    Task<ApiResponse<Guid>> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<ProdutoImagemResponse>> UploadImageAsync(Guid produtoId, FileResult file, CancellationToken ct = default);
}
