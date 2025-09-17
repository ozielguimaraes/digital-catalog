using System.Text.Json;
using MeuCatalogo.Features.Auth.Requests;
using MeuCatalogo.Features.Auth.Responses;
using Microsoft.Extensions.Logging;
using Refit;

namespace MeuCatalogo.Features.Auth.ApiClients;

public class AuthService(ILogger<AuthService> logger, IAuthApi authApi) : BaseApiService, IAuthService
{
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
            logger.LogInformation("Iniciando cadastro de usuário.");
            var response = await authApi.SignupAsync(request, cancellationToken);

            logger.LogInformation("Cadastro concluído com sucesso para o login: {Email}", request.Email);
            return ApiResponse<UserResponse>.Success(response);
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro da API ao cadastrar usuário.");
            return ApiResponse<UserResponse>.Error("Erro ao efetuar cadastro", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao efetuar o cadastro.");
            return ApiResponse<UserResponse>.Error("Erro inesperado ao efetuar o cadastro.");
        }
    }

    public async Task<ApiResponse<SigninResponse>> SigninAsync(SigninRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Tentando login para o e-mail: {Email}", request.Email);
            var response = await authApi.SigninAsync(request, cancellationToken);

            if (!string.IsNullOrWhiteSpace(response.Token))
            {
                await SetUserInfo(response);
                logger.LogInformation("Login bem-sucedido para o e-mail: {Email}", response.User.Email);
                return ApiResponse<SigninResponse>.Success(response);
            }

            logger.LogWarning("Login falhou: token vazio para o e-mail: {Email}", request.Email);
            return ApiResponse<SigninResponse>.Error("Usuário ou senha incorreta.");
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro da API ao efetuar login para o e-mail: {Email}", request.Email);
            return ApiResponse<SigninResponse>.Error("Erro ao efetuar login", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao fazer login.");
            return ApiResponse<SigninResponse>.Error("Erro inesperado ao fazer login.");
        }
    }

    private static async Task SetUserInfo(SigninResponse response)
    {
        string json = JsonSerializer.Serialize(response.User);
        Preferences.Set(UserInfoKey, json);
        Preferences.Set(TokenKey, response.Token);

        await Task.CompletedTask;
    }
}
