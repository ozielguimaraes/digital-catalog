using MeuCatalogo.Features.Produto.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Produto.Data.Remote.Contracts.Responses;

namespace MeuCatalogo.Features.Produto.Data.Remote;

public interface IProdutoRemoteDataSource
{
    Task<ApiResponse<ICollection<ProdutoResponse>>> ObterPorCatalogoIdAsync(Guid catalogoId, CancellationToken ct = default);
    Task<ApiResponse<ProdutoResponse>> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<ProdutoResponse>> CreateAsync(ProdutoCreateRequest request, CancellationToken ct = default);
    Task<ApiResponse<ProdutoResponse>> UpdateAsync(Guid id, ProdutoUpdateRequest request, CancellationToken ct = default);
    Task<ApiResponse<Guid>> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<ProdutoImagemResponse>> UploadImageAsync(Guid produtoId, FileResult file, CancellationToken ct = default);
}

