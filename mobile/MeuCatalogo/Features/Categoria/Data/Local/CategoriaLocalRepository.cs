using MeuCatalogo.Features.Categoria.Domain;
using MeuCatalogo.Infrastructure.Database;

namespace MeuCatalogo.Features.Categoria.Data.Local;

public sealed class CategoriaLocalRepository(AppDbContext dbContext)
    : BaseRepository<CategoriaEntity>(dbContext), ICategoriaLocalRepository
{
    public async Task<IReadOnlyList<CategoriaEntity>> GetByCatalogoIdAsync(string catalogoId)
    {
        await _dbContext.InitializeAsync();

        return await _dbContext.Database.Table<CategoriaEntity>()
            .Where(c => c.CatalogoId == catalogoId)
            .OrderBy(c => c.Nome)
            .ToListAsync();
    }

    public async Task ReplaceByCatalogoIdAsync(string catalogoId, IEnumerable<CategoriaEntity> categorias)
    {
        await _dbContext.InitializeAsync();

        await _dbContext.Database.RunInTransactionAsync(database =>
        {
            database.Execute("DELETE FROM Categorias WHERE CatalogoId = ?", catalogoId);
            database.InsertAll(categorias);
        });
    }

    public async Task UpsertAsync(CategoriaEntity categoria)
    {
        await _dbContext.InitializeAsync();

        var existing = await _dbContext.Database.Table<CategoriaEntity>()
            .Where(c => c.Id == categoria.Id)
            .FirstOrDefaultAsync();

        if (existing == null)
        {
            categoria.CreatedAt = DateTime.UtcNow;
            await _dbContext.Database.InsertAsync(categoria);
            return;
        }

        categoria.CreatedAt = existing.CreatedAt;
        categoria.UpdatedAt = DateTime.UtcNow;
        await _dbContext.Database.UpdateAsync(categoria);
    }
}

