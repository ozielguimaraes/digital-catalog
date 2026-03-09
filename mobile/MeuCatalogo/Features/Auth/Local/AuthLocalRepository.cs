using MeuCatalogo.Features.Auth.Local.Entities;
using MeuCatalogo.Features.Auth.Responses;
using MeuCatalogo.Infrastructure.LocalData;

namespace MeuCatalogo.Features.Auth.Local;

public sealed class AuthLocalRepository : BaseLocalRepository, IAuthLocalRepository
{
    public async Task SaveUserSessionAsync(UserResponse user, CancellationToken ct = default)
    {
        await EnsureInitializedAsync();

        var currentSessions = await Db.Table<UserSessionLocalEntity>().ToListAsync();
        foreach (var session in currentSessions.Where(s => s.UserId != user.Id))
        {
            await Db.DeleteAsync(session);
        }

        var entity = new UserSessionLocalEntity
        {
            UserId = user.Id,
            Nome = user.Nome,
            Email = user.Email,
            UserName = user.UserName,
            LastLoginAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await Db.InsertOrReplaceAsync(entity);
    }

    protected override async Task CreateSchemaAsync()
    {
        await Db.CreateTableAsync<UserSessionLocalEntity>();
        await Db.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_user_sessions_email ON user_sessions(Email)");
    }
}
