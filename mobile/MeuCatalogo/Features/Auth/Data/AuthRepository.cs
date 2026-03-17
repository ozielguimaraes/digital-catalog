using MeuCatalogo.Features.Auth.Data.Local;
using MeuCatalogo.Features.Auth.Data.Remote;
using MeuCatalogo.Features.Auth.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Auth.Data.Remote.Contracts.Responses;
using MeuCatalogo.Features.Auth.Domain;

namespace MeuCatalogo.Features.Auth.Data;

public sealed class AuthRepository(IAuthRemoteDataSource remote, IAuthLocalDataSource local, IUserRepository userRepository)
    : IAuthRepository
{
    public bool IsAuthenticated()
    {
        return local.GetIsAuthenticatedFlag();
    }

    public async Task<bool> IsAuthenticatedAsync(CancellationToken ct = default)
    {
        var token = await local.GetAccessTokenAsync(ct);
        return !string.IsNullOrEmpty(token);
    }

    public Task<ApiResponse<UserResponse>> SignupAsync(SignupRequest request, CancellationToken ct = default)
    {
        return remote.SignupAsync(request, ct);
    }

    public async Task<ApiResponse<SigninResponse>> SigninAsync(SigninRequest request, CancellationToken ct = default)
    {
        var response = await remote.SigninAsync(request, ct);
        if (response is not { RetornouComSucesso: true, Dados: not null })
        {
            local.SetIsAuthenticatedFlag(false);
            return response;
        }

        if (string.IsNullOrWhiteSpace(response.Dados.Token))
        {
            local.SetIsAuthenticatedFlag(false);
            return ApiResponse<SigninResponse>.Error("Usuário ou senha incorreta.");
        }

        await local.SetTokensAsync(response.Dados.Token, response.Dados.RefreshToken, ct);
        local.SetIsAuthenticatedFlag(true);

        if (response.Dados?.User == null)
        {
            return response;
        }

        var userResponse = response.Dados.User;
        var userEntity = new UserEntity
        {
            Id = userResponse.Id.ToString(),
            Nome = userResponse.Nome,
            Email = userResponse.Email,
            UserName = userResponse.UserName
        };

        await userRepository.SetCurrentUserAsync(userEntity);

        return response;
    }

    public async Task<bool> RefreshTokenAsync(CancellationToken ct = default)
    {
        var refreshToken = await local.GetRefreshTokenAsync(ct);
        if (string.IsNullOrEmpty(refreshToken))
        {
            local.SetIsAuthenticatedFlag(false);
            return false;
        }

        var apiResponse = await remote.RefreshTokenAsync(new RefreshTokenRequest { RefreshToken = refreshToken }, ct);

        if (apiResponse is { RetornouComSucesso: true, Dados: not null } && !string.IsNullOrWhiteSpace(apiResponse.Dados.Token))
        {
            await local.SetTokensAsync(apiResponse.Dados.Token, apiResponse.Dados.RefreshToken, ct);
            local.SetIsAuthenticatedFlag(true);
            return true;
        }

        local.SetIsAuthenticatedFlag(false);
        return false;
    }

    public async Task LogoutAsync(CancellationToken ct = default)
    {
        local.ClearTokens();
        local.SetIsAuthenticatedFlag(false);
        await userRepository.ClearUserAsync();
    }
}
