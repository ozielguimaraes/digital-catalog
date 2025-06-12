namespace MeuCatalogo.Features.Auth.Responses;

public class UserResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
    public string UserName { get; set; }
}
