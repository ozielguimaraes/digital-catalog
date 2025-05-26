using MeuCatalogo.Features.Auth.Requests;
using MeuCatalogo.Features.Auth.Responses;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Features.Auth.ApiClients;

public class AuthService : IAuthService
{
    private const string TokenKey = "auth_token";
    private const string IdKey = "id_key";
    private const string EmailKey = "email_key";
    private readonly IAuthApi _authApi;
    private readonly ILogger<AuthService> _logger;

    public AuthService(ILogger<AuthService> logger, IAuthApi authApi)
    {
        _logger = logger;
        _authApi = authApi;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        string? token = await SecureStorage.GetAsync(TokenKey);
        return !string.IsNullOrEmpty(token);
    }

    public bool IsAuthenticated()
    {
        string? token = SecureStorage.GetAsync(TokenKey).Result;
        return !string.IsNullOrEmpty(token);
    }

    public async Task<UserResponse?> SignupAsync(SignupRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _authApi.SignupAsync(request, cancellationToken);

            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task<SigninResponse> SigninAsync(SigninRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _authApi.SigninAsync(request, cancellationToken);

            if (!string.IsNullOrWhiteSpace(response.Token))
            {
                await SecureStorage.SetAsync(TokenKey, response.Token);
                await SecureStorage.SetAsync(EmailKey, response.User.Email);
                await SecureStorage.SetAsync(IdKey, response.User.Id.ToString());
                return response;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro no login: {ex.Message}");
        }

        //TODO Use a better way of handling erros...
        throw new Exception("Usuário ou senha incorretos");
    }
}
