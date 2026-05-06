using MeuCatalogo.Features.Auth.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Auth.Data.Remote.Contracts.Responses;
using Microsoft.Extensions.Logging;
using Refit;

namespace MeuCatalogo.Features.Auth.Data.Remote;

public sealed class AuthRemoteDataSource(
    ILogger<AuthRemoteDataSource> logger,
    IAuthApi authApi) : BaseApiService, IAuthRemoteDataSource
{
    public async Task<ApiResponse<UserResponse>> SignupAsync(SignupRequest request, CancellationToken ct = default)
    {
        try
        {
            logger.LogInformation("Iniciando cadastro de usuário.");
            var response = await authApi.SignupAsync(request, ct);

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

    public async Task<ApiResponse<SigninResponse>> SigninAsync(SigninRequest request, CancellationToken ct = default)
    {
        try
        {
            logger.LogInformation("Tentando login para o e-mail: {Email}", request.Email);
            var response = await authApi.SigninAsync(request, ct);
            return ApiResponse<SigninResponse>.Success(response);
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

    public async Task<ApiResponse<RefreshTokenResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default)
    {
        try
        {
            var response = await authApi.RefreshTokenAsync(request, ct);
            return ApiResponse<RefreshTokenResponse>.Success(response);
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro da API ao atualizar token.");
            var errorMessage = ObterMensagemErroApi(apiEx);
            return ApiResponse<RefreshTokenResponse>.Error(errorMessage, GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao atualizar token.");
            return ApiResponse<RefreshTokenResponse>.Error("Erro inesperado ao atualizar token.");
        }
    }
}
