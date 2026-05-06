namespace MeuCatalogo.Features.Auth.Data.Remote.Contracts.Requests;

public record SignupRequest(string Nome, string Email, string UserName, string Password);

