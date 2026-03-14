using MeuCatalogo.Features.Produto.Domain;
using MeuCatalogo.Infrastructure.Database;

namespace MeuCatalogo.Features.Produto.Data.Local;

public sealed class ProdutoImagemLocalRepository : IProdutoImagemLocalRepository
{
    private readonly AppDbContext _dbContext;

    public ProdutoImagemLocalRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ProdutoImagemEntity>> GetByProdutoIdAsync(string produtoId)
    {
        await _dbContext.InitializeAsync();
        return await _dbContext.Database.Table<ProdutoImagemEntity>()
            .Where(i => i.ProdutoId == produtoId)
            .OrderBy(i => i.Ordem)
            .ToListAsync();
    }

    public async Task ReplaceByCatalogoIdAsync(string catalogoId, IEnumerable<ProdutoImagemEntity> imagens)
    {
        await _dbContext.InitializeAsync();

        var list = imagens.ToList();
        await _dbContext.Database.RunInTransactionAsync(database =>
        {
            database.Execute("DELETE FROM ProdutoImagens WHERE CatalogoId = ?", catalogoId);
            database.InsertAll(list);
        });
    }

    public async Task ReplaceByProdutoIdAsync(string produtoId, IEnumerable<ProdutoImagemEntity> imagens)
    {
        await _dbContext.InitializeAsync();

        var list = imagens.ToList();
        await _dbContext.Database.RunInTransactionAsync(database =>
        {
            database.Execute("DELETE FROM ProdutoImagens WHERE ProdutoId = ?", produtoId);
            database.InsertAll(list);
        });
    }
}
