using System.Text.Json;
using MeuCatalogo.Features.Auth.Local;
using MeuCatalogo.Features.Auth.Requests;
using MeuCatalogo.Features.Auth.Responses;
using Microsoft.Extensions.Logging;
using Refit;

namespace MeuCatalogo.Features.Auth.ApiClients;

public class AuthService(
    ILogger<AuthService> logger,
    IAuthApi authApi,
    IAuthLocalRepository authLocalRepository) : BaseApiService, IAuthService
{
    public async Task<bool> IsAuthenticatedAsync()
    {
        string? token = Preferences.Get(TokenKey, string.Empty);
        return await Task.FromResult(!string.IsNullOrEmpty(token));
    }

    public bool IsAuthenticated()
    {
        string? token = Preferences.Get(TokenKey, string.Empty);
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
            var errorMessage = ObterMensagemErroApi(apiEx);
            return ApiResponse<UserResponse>.Error(errorMessage, GetProblemDetails(apiEx));
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
                await SetUserInfo(response, cancellationToken);
                logger.LogInformation("Login bem-sucedido para o e-mail: {Email}", response.User.Email);
                return ApiResponse<SigninResponse>.Success(response);
            }

            logger.LogWarning("Login falhou: token vazio para o e-mail: {Email}", request.Email);
            return ApiResponse<SigninResponse>.Error("Usuário ou senha incorreta.");
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro da API ao efetuar login para o e-mail: {Email}", request.Email);
            var errorMessage = ObterMensagemErroApi(apiEx);
            return ApiResponse<SigninResponse>.Error(errorMessage, GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao fazer login.");
            return ApiResponse<SigninResponse>.Error("Erro inesperado ao fazer login.");
        }
    }

    public async Task<bool> RefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var refreshToken = Preferences.Get(RefreshTokenKey, string.Empty);
            if (string.IsNullOrEmpty(refreshToken))
            {
                logger.LogWarning("Refresh token não encontrado.");
                return false;
            }

            var request = new RefreshTokenRequest { RefreshToken = refreshToken };
            var response = await authApi.RefreshTokenAsync(request, cancellationToken);

            if (response != null && !string.IsNullOrEmpty(response.Token))
            {
                Preferences.Set(TokenKey, response.Token);
                if (!string.IsNullOrEmpty(response.RefreshToken))
                {
                    Preferences.Set(RefreshTokenKey, response.RefreshToken);
                }
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao atualizar token.");
            return false;
        }
    }

    private async Task SetUserInfo(SigninResponse response, CancellationToken cancellationToken)
    {
        string json = JsonSerializer.Serialize(response.User);
        Preferences.Set(UserInfoKey, json);
        Preferences.Set(TokenKey, response.Token);
        if (!string.IsNullOrEmpty(response.RefreshToken))
        {
            Preferences.Set(RefreshTokenKey, response.RefreshToken);
        }

        await authLocalRepository.SaveUserSessionAsync(response.User, cancellationToken);
    }
}
