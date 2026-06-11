using MeuCatalogo.Domain.Enums;
using MeuCatalogo.Features.Produto.Domain;
using MeuCatalogo.Infrastructure.Database;

namespace MeuCatalogo.Features.Produto.Data.Local;

public sealed class ProdutoVariacaoLocalRepository(AppDbContext dbContext) : IProdutoVariacaoLocalRepository
{
    public async Task<IReadOnlyList<ProdutoVariacaoEntity>> GetByProdutoIdAsync(string produtoId)
    {
        await dbContext.InitializeAsync();
        return await dbContext.Database.Table<ProdutoVariacaoEntity>()
            .Where(v => v.ProdutoId == produtoId)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ProdutoVariacaoEntity>> GetByCatalogoIdAsync(string catalogoId)
    {
        await dbContext.InitializeAsync();
        return await dbContext.Database.Table<ProdutoVariacaoEntity>()
            .Where(v => v.CatalogoId == catalogoId)
            .ToListAsync();
    }

    public async Task ReplaceByCatalogoIdAsync(string catalogoId, IEnumerable<ProdutoVariacaoEntity> variacoes)
    {
        await dbContext.InitializeAsync();

        var list = variacoes.ToList();
        await dbContext.Database.RunInTransactionAsync(database =>
        {
            // Preserva variações adicionadas offline (ainda não sincronizadas) ao puxar do servidor.
            var pendentesIds = database.Query<ProdutoVariacaoEntity>(
                    "SELECT Id FROM ProdutoVariacoes WHERE CatalogoId = ? AND SyncStatus <> ?",
                    catalogoId, (int)SyncStatus.Completed)
                .Select(v => v.Id)
                .ToHashSet();

            database.Execute("DELETE FROM ProdutoVariacoes WHERE CatalogoId = ? AND SyncStatus = ?",
                catalogoId, (int)SyncStatus.Completed);

            database.InsertAll(list.Where(v => !pendentesIds.Contains(v.Id)));
        });
    }

    public async Task ReplaceByProdutoIdAsync(string produtoId, IEnumerable<ProdutoVariacaoEntity> variacoes)
    {
        await dbContext.InitializeAsync();

        var list = variacoes.ToList();
        await dbContext.Database.RunInTransactionAsync(database =>
        {
            database.Execute("DELETE FROM ProdutoVariacoes WHERE ProdutoId = ?", produtoId);
            database.InsertAll(list);
        });
    }

    public async Task DeleteByProdutoIdAsync(string produtoId)
    {
        await dbContext.InitializeAsync();
        await dbContext.Database.ExecuteAsync("DELETE FROM ProdutoVariacoes WHERE ProdutoId = ?", produtoId);
    }
}
