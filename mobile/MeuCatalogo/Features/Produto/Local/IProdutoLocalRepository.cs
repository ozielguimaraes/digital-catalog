using MeuCatalogo.Features.Produto.Responses;

namespace MeuCatalogo.Features.Produto.Local;

public interface IProdutoLocalRepository
{
    Task SaveCatalogoProdutosFromRemoteAsync(Guid catalogoId, IEnumerable<ProdutoResponse> produtos, CancellationToken ct = default);
    Task SaveProdutoFromRemoteAsync(ProdutoResponse produto, CancellationToken ct = default);
    Task SaveProdutoOfflineAsync(ProdutoResponse produto, LocalSyncStatus syncStatus, CancellationToken ct = default);
    Task SaveProdutoImagemOfflineAsync(Guid produtoId, ProdutoImagemResponse imagem, LocalSyncStatus syncStatus, CancellationToken ct = default);
    Task<ICollection<ProdutoResponse>> GetProdutosByCatalogoIdAsync(Guid catalogoId, CancellationToken ct = default);
    Task<ProdutoResponse?> GetProdutoByIdAsync(Guid produtoId, CancellationToken ct = default);
    Task MarkProdutoAsDeletedAsync(Guid produtoId, CancellationToken ct = default);
    Task RemoveProdutoAsync(Guid produtoId, CancellationToken ct = default);
}
