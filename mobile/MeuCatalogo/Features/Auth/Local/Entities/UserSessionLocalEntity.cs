using SQLite;

namespace MeuCatalogo.Features.Auth.Local.Entities;

[Table("user_sessions")]
public sealed class UserSessionLocalEntity
{
    [PrimaryKey]
    public Guid UserId { get; set; }

    [Indexed]
    public string Email { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime LastLoginAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
