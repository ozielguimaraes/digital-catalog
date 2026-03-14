using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Core.Base;

namespace MeuCatalogo.Infrastructure.Database;

public abstract class BaseRepository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity, new()
{
    protected readonly AppDbContext _dbContext;

    protected BaseRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public virtual async Task<TEntity?> GetByIdAsync(string id)
    {
        await _dbContext.InitializeAsync();
        return await _dbContext.Database.Table<TEntity>().Where(e => e.Id == id).FirstOrDefaultAsync();
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        await _dbContext.InitializeAsync();
        return await _dbContext.Database.Table<TEntity>().ToListAsync();
    }

    public virtual async Task AddAsync(TEntity entity)
    {
        await _dbContext.InitializeAsync();
        entity.CreatedAt = DateTime.UtcNow;
        await _dbContext.Database.InsertAsync(entity);
    }

    public virtual async Task UpdateAsync(TEntity entity)
    {
        await _dbContext.InitializeAsync();
        entity.UpdatedAt = DateTime.UtcNow;
        await _dbContext.Database.UpdateAsync(entity);
    }

    public virtual async Task DeleteAsync(TEntity entity)
    {
        await _dbContext.InitializeAsync();
        await _dbContext.Database.DeleteAsync(entity);
    }
}
