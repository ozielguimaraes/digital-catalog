namespace MeuCatalogo.Features.Auth.Responses;

public class SigninResponse
{
    public string Token { get; set; }
    public UserResponse User { get; set; }
}
