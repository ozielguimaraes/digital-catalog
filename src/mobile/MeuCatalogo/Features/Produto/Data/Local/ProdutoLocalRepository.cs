using MeuCatalogo.Features.Produto.Domain;
using MeuCatalogo.Infrastructure.Database;

namespace MeuCatalogo.Features.Produto.Data.Local;

public sealed class ProdutoLocalRepository(AppDbContext dbContext)
    : BaseRepository<ProdutoEntity>(dbContext), IProdutoLocalRepository
{
    public async Task<IEnumerable<ProdutoEntity>> GetByCatalogoIdAsync(string catalogoId)
    {
        await _dbContext.InitializeAsync();
        return await _dbContext.Database.Table<ProdutoEntity>()
            .Where(p => p.CatalogoId == catalogoId)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProdutoEntity>> GetByCategoriaIdAsync(string categoriaId)
    {
        await _dbContext.InitializeAsync();
        return await _dbContext.Database.Table<ProdutoEntity>()
            .Where(p => p.CategoriaId == categoriaId)
            .ToListAsync();
    }

    public async Task ReplaceByCatalogoIdAsync(string catalogoId, IEnumerable<ProdutoEntity> produtos)
    {
        await _dbContext.InitializeAsync();

        await _dbContext.Database.RunInTransactionAsync(database =>
        {
            database.Execute("DELETE FROM Produtos WHERE CatalogoId = ?", catalogoId);
            database.InsertAll(produtos);
        });
    }

    public async Task SyncWithRemoteAsync()
    {
        await Task.CompletedTask;
    }
}
