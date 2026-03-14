using MeuCatalogo.Features.Produto.Data.Remote;
using MeuCatalogo.Features.Produto.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Produto.Data.Remote.Contracts.Responses;

namespace MeuCatalogo.Features.Produto.Data;

public sealed class ProdutoRepository : IProdutoRepository
{
    private readonly IProdutoRemoteDataSource _remote;

    public ProdutoRepository(IProdutoRemoteDataSource remote)
    {
        _remote = remote;
    }

    public Task<ApiResponse<ICollection<ProdutoResponse>>> ObterPorCatalogoIdAsync(Guid catalogoId, CancellationToken ct = default)
        => _remote.ObterPorCatalogoIdAsync(catalogoId, ct);

    public Task<ApiResponse<ProdutoResponse>> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => _remote.ObterPorIdAsync(id, ct);

    public Task<ApiResponse<ProdutoResponse>> CreateAsync(ProdutoCreateRequest request, CancellationToken ct = default)
        => _remote.CreateAsync(request, ct);

    public Task<ApiResponse<ProdutoResponse>> UpdateAsync(Guid id, ProdutoUpdateRequest request, CancellationToken ct = default)
        => _remote.UpdateAsync(id, request, ct);

    public Task<ApiResponse<Guid>> DeleteAsync(Guid id, CancellationToken ct = default)
        => _remote.DeleteAsync(id, ct);

    public Task<ApiResponse<ProdutoImagemResponse>> UploadImageAsync(Guid produtoId, FileResult file, CancellationToken ct = default)
        => _remote.UploadImageAsync(produtoId, file, ct);
}

