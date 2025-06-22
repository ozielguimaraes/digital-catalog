using MeuCatalogo.Features.Produto.Requests;
using MeuCatalogo.Features.Produto.Responses;

namespace MeuCatalogo.Features.Produto;

public interface IProdutoService
{
    Task<ApiResponse<ICollection<ProdutoResponse>>> ObterPorCatalogoIdAsync(Guid catalogoId, CancellationToken ct = default);
    Task<ApiResponse<ProdutoResponse>> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<ProdutoResponse>> CreateAsync(ProdutoCreateRequest request, CancellationToken ct = default);
    Task<ApiResponse<ProdutoResponse>> UpdateCatalogoAsync(Guid id, ProdutoUpdateRequest request, CancellationToken ct = default);
    Task<ApiResponse<Guid>> DeleteCatalogoAsync(Guid id, CancellationToken ct = default);
}

public sealed class ProdutoService : IProdutoService
{
    public Task<ApiResponse<ICollection<ProdutoResponse>>> ObterPorCatalogoIdAsync(Guid catalogoId, CancellationToken ct = default) => throw new NotImplementedException();

    public Task<ApiResponse<ProdutoResponse>> ObterPorIdAsync(Guid id, CancellationToken ct = default) => throw new NotImplementedException();

    public Task<ApiResponse<ProdutoResponse>> CreateAsync(ProdutoCreateRequest request, CancellationToken ct = default) => throw new NotImplementedException();

    public Task<ApiResponse<ProdutoResponse>> UpdateCatalogoAsync(Guid id, ProdutoUpdateRequest request, CancellationToken ct = default) => throw new NotImplementedException();

    public Task<ApiResponse<Guid>> DeleteCatalogoAsync(Guid id, CancellationToken ct = default) => throw new NotImplementedException();
}
