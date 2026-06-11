using MeuCatalogo.Domain.Enums;
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

        var list = produtos.ToList();
        await _dbContext.Database.RunInTransactionAsync(database =>
        {
            // Preserva produtos criados/editados offline (ainda não sincronizados):
            // o pull do servidor não pode apagá-los antes de subirem.
            var pendentesIds = database.Query<ProdutoEntity>(
                    "SELECT Id FROM Produtos WHERE CatalogoId = ? AND SyncStatus <> ?",
                    catalogoId, (int)SyncStatus.Completed)
                .Select(p => p.Id)
                .ToHashSet();

            database.Execute("DELETE FROM Produtos WHERE CatalogoId = ? AND SyncStatus = ?",
                catalogoId, (int)SyncStatus.Completed);

            database.InsertAll(list.Where(p => !pendentesIds.Contains(p.Id)));
        });
    }

    public async Task SyncWithRemoteAsync()
    {
        await Task.CompletedTask;
    }
}
