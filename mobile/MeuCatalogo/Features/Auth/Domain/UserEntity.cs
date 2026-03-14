using MeuCatalogo.Core.Base;

namespace MeuCatalogo.Features.Auth.Domain;

public class UserEntity : BaseEntity
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;

    // Optional flags or tokens we might want to store securely offline
    // but the actual Token/RefreshToken stays in SecureStorage
}
