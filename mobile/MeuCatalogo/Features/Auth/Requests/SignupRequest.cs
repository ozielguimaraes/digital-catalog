namespace MeuCatalogo.Features.Auth.Requests;

public record SignupRequest(string Nome, string Email, string UserName, string Password);
