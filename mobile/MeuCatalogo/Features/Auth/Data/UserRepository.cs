using MeuCatalogo.Features.Auth.Domain;
using MeuCatalogo.Infrastructure.Database;

namespace MeuCatalogo.Features.Auth.Data;

public class UserRepository : BaseRepository<UserEntity>, IUserRepository
{
    public UserRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<UserEntity?> GetCurrentUserAsync()
    {
        await _dbContext.InitializeAsync();
        // Assume single logged-in user per device for offline mode
        return await _dbContext.Database.Table<UserEntity>().FirstOrDefaultAsync();
    }

    public async Task SetCurrentUserAsync(UserEntity user)
    {
        await _dbContext.InitializeAsync();

        var existingUser = await GetCurrentUserAsync();
        if (existingUser != null)
        {
            if (existingUser.Id != user.Id)
            {
                await ClearUserAsync();
                await AddAsync(user);
            }
            else
            {
                user.CreatedAt = existingUser.CreatedAt;
                await UpdateAsync(user);
            }
        }
        else
        {
            await AddAsync(user);
        }
    }

    public async Task ClearUserAsync()
    {
        await _dbContext.InitializeAsync();
        await _dbContext.Database.DeleteAllAsync<UserEntity>();
    }
}
