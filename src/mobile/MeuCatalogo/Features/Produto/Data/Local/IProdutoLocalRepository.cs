using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Produto.Domain;

namespace MeuCatalogo.Features.Produto.Data.Local;

public interface IProdutoLocalRepository : IRepository<ProdutoEntity>
{
    Task<IEnumerable<ProdutoEntity>> GetByCatalogoIdAsync(string catalogoId);
    Task<IEnumerable<ProdutoEntity>> GetByCategoriaIdAsync(string categoriaId);
    Task ReplaceByCatalogoIdAsync(string catalogoId, IEnumerable<ProdutoEntity> produtos);
    Task SyncWithRemoteAsync();
}
