using MeuCatalogo.Features.Catalogo.Domain;
using MeuCatalogo.Infrastructure.Database;

namespace MeuCatalogo.Features.Catalogo.Data.Local;

public sealed class CatalogoLocalRepository : BaseRepository<CatalogoEntity>, ICatalogoLocalRepository
{
    public CatalogoLocalRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task ReplaceAllAsync(IEnumerable<CatalogoEntity> catalogos)
    {
        await _dbContext.InitializeAsync();

        await _dbContext.Database.RunInTransactionAsync(database =>
        {
            database.DeleteAll<CatalogoEntity>();
            database.InsertAll(catalogos);
        });
    }
}

