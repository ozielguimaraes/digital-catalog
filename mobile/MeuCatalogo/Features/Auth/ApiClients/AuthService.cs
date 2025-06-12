using MeuCatalogo.Features.Auth.Requests;
using MeuCatalogo.Features.Auth.Responses;
using Microsoft.Extensions.Logging;
using Refit;

namespace MeuCatalogo.Features.Auth.ApiClients;

public class AuthService(ILogger<AuthService> logger, IAuthApi authApi) : IAuthService
{
    private const string TokenKey = "auth_token";
    private const string IdKey = "id_key";
    private const string EmailKey = "email_key";

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

    public async Task<ApiResponse<UserResponse>> SignupAsync(SignupRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await authApi.SignupAsync(request, cancellationToken);

            return ApiResponse<UserResponse>.Success(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApiResponse<UserResponse>.Error("Erro inesperado ao efetuar o cadastro");
        }
    }

    public async Task<ApiResponse<SigninResponse>> SigninAsync(SigninRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await authApi.SigninAsync(request, cancellationToken);

            if (!string.IsNullOrWhiteSpace(response.Token))
            {
                await SecureStorage.SetAsync(TokenKey, response.Token);
                await SecureStorage.SetAsync(EmailKey, response.User.Email);
                await SecureStorage.SetAsync(IdKey, response.User.Id.ToString());

                return ApiResponse<SigninResponse>.Success(response);
            }
        }
        catch (ApiException apiEx)
        {
            var errorContent = await apiEx.GetContentAsAsync<ApiResponse<SigninResponse>>();
            return ApiResponse<SigninResponse>.Error(errorContent?.MensageDeErro ?? "Erro desconhecido ao fazer login.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApiResponse<SigninResponse>.Error("Erro desconhecido ao fazer login.");
        }

        return ApiResponse<SigninResponse>.Error("Usuário ou senha incorreta.");
    }
}
