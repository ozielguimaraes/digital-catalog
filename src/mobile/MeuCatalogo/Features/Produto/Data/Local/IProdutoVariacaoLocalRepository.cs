using MeuCatalogo.Features.Produto.Domain;

namespace MeuCatalogo.Features.Produto.Data.Local;

public interface IProdutoVariacaoLocalRepository
{
    Task<IReadOnlyList<ProdutoVariacaoEntity>> GetByProdutoIdAsync(string produtoId);
    Task<IReadOnlyList<ProdutoVariacaoEntity>> GetByCatalogoIdAsync(string catalogoId);
    Task ReplaceByCatalogoIdAsync(string catalogoId, IEnumerable<ProdutoVariacaoEntity> variacoes);
    Task ReplaceByProdutoIdAsync(string produtoId, IEnumerable<ProdutoVariacaoEntity> variacoes);
    Task DeleteByProdutoIdAsync(string produtoId);
}
