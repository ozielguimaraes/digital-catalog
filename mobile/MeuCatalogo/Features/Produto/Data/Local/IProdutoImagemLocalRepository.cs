using MeuCatalogo.Features.Produto.Domain;

namespace MeuCatalogo.Features.Produto.Data.Local;

public interface IProdutoImagemLocalRepository
{
    Task<IReadOnlyList<ProdutoImagemEntity>> GetByProdutoIdAsync(string produtoId);
    Task ReplaceByCatalogoIdAsync(string catalogoId, IEnumerable<ProdutoImagemEntity> imagens);
    Task ReplaceByProdutoIdAsync(string produtoId, IEnumerable<ProdutoImagemEntity> imagens);
}
