using MeuCatalogo.Features.Produto.Data.Remote;
using MeuCatalogo.Features.Produto.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Produto.Data.Remote.Contracts.Responses;

namespace MeuCatalogo.Features.Produto.Data;

public sealed class ProdutoRepository(IProdutoRemoteDataSource remote) : IProdutoRepository
{
    public Task<ApiResponse<ICollection<ProdutoResponse>>> ObterPorCatalogoIdAsync(Guid catalogoId, CancellationToken ct = default)
        => remote.ObterPorCatalogoIdAsync(catalogoId, ct);

    public Task<ApiResponse<ProdutoResponse>> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => remote.ObterPorIdAsync(id, ct);

    public Task<ApiResponse<ProdutoResponse>> CreateAsync(ProdutoCreateRequest request, CancellationToken ct = default)
        => remote.CreateAsync(request, ct);

    public Task<ApiResponse<ProdutoResponse>> UpdateAsync(Guid id, ProdutoUpdateRequest request, CancellationToken ct = default)
        => remote.UpdateAsync(id, request, ct);

    public Task<ApiResponse<Guid>> DeleteAsync(Guid id, CancellationToken ct = default)
        => remote.DeleteAsync(id, ct);

    public Task<ApiResponse<ProdutoImagemResponse>> UploadImageAsync(Guid produtoId, FileResult file, CancellationToken ct = default)
        => remote.UploadImageAsync(produtoId, file, ct);
}

