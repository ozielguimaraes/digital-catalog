using MeuCatalogo.Features.Produto.Domain;
using MeuCatalogo.Infrastructure.Database;

namespace MeuCatalogo.Features.Produto.Data.Local;

public sealed class ProdutoImagemLocalRepository(AppDbContext dbContext) : IProdutoImagemLocalRepository
{
    public async Task<IReadOnlyList<ProdutoImagemEntity>> GetByProdutoIdAsync(string produtoId)
    {
        await dbContext.InitializeAsync();
        return await dbContext.Database.Table<ProdutoImagemEntity>()
            .Where(i => i.ProdutoId == produtoId)
            .OrderBy(i => i.Ordem)
            .ToListAsync();
    }

    public async Task ReplaceByCatalogoIdAsync(string catalogoId, IEnumerable<ProdutoImagemEntity> imagens)
    {
        await dbContext.InitializeAsync();

        var list = imagens.ToList();
        await dbContext.Database.RunInTransactionAsync(database =>
        {
            database.Execute("DELETE FROM ProdutoImagens WHERE CatalogoId = ?", catalogoId);
            database.InsertAll(list);
        });
    }

    public async Task ReplaceByProdutoIdAsync(string produtoId, IEnumerable<ProdutoImagemEntity> imagens)
    {
        await dbContext.InitializeAsync();

        var list = imagens.ToList();
        await dbContext.Database.RunInTransactionAsync(database =>
        {
            database.Execute("DELETE FROM ProdutoImagens WHERE ProdutoId = ?", produtoId);
            database.InsertAll(list);
        });
    }

    public async Task DeleteByProdutoIdAsync(string produtoId)
    {
        await dbContext.InitializeAsync();
        await dbContext.Database.ExecuteAsync("DELETE FROM ProdutoImagens WHERE ProdutoId = ?", produtoId);
    }
}
