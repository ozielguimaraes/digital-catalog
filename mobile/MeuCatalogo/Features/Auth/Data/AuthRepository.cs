using MeuCatalogo.Features.Auth.Data.Local;
using MeuCatalogo.Features.Auth.Data.Remote;
using MeuCatalogo.Features.Auth.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Auth.Data.Remote.Contracts.Responses;
using MeuCatalogo.Features.Auth.Domain;

namespace MeuCatalogo.Features.Auth.Data;

public sealed class AuthRepository : IAuthRepository
{
    private readonly IAuthRemoteDataSource _remote;
    private readonly IAuthLocalDataSource _local;
    private readonly IUserRepository _userRepository;

    public AuthRepository(IAuthRemoteDataSource remote, IAuthLocalDataSource local, IUserRepository userRepository)
    {
        _remote = remote;
        _local = local;
        _userRepository = userRepository;
    }

    public bool IsAuthenticated()
    {
        return _local.GetIsAuthenticatedFlag();
    }

    public async Task<bool> IsAuthenticatedAsync(CancellationToken ct = default)
    {
        var token = await _local.GetAccessTokenAsync(ct);
        return !string.IsNullOrEmpty(token);
    }

    public Task<ApiResponse<UserResponse>> SignupAsync(SignupRequest request, CancellationToken ct = default)
    {
        return _remote.SignupAsync(request, ct);
    }

    public async Task<ApiResponse<SigninResponse>> SigninAsync(SigninRequest request, CancellationToken ct = default)
    {
        var response = await _remote.SigninAsync(request, ct);
        if (response is not { RetornouComSucesso: true, Dados: not null })
        {
            _local.SetIsAuthenticatedFlag(false);
            return response;
        }

        if (string.IsNullOrWhiteSpace(response.Dados.Token))
        {
            _local.SetIsAuthenticatedFlag(false);
            return ApiResponse<SigninResponse>.Error("Usuário ou senha incorreta.");
        }

        await _local.SetTokensAsync(response.Dados.Token, response.Dados.RefreshToken, ct);
        _local.SetIsAuthenticatedFlag(true);

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

        await _userRepository.SetCurrentUserAsync(userEntity);

        return response;
    }

    public async Task<bool> RefreshTokenAsync(CancellationToken ct = default)
    {
        var refreshToken = await _local.GetRefreshTokenAsync(ct);
        if (string.IsNullOrEmpty(refreshToken))
        {
            _local.SetIsAuthenticatedFlag(false);
            return false;
        }

        var apiResponse = await _remote.RefreshTokenAsync(new RefreshTokenRequest { RefreshToken = refreshToken }, ct);

        if (apiResponse is { RetornouComSucesso: true, Dados: not null } && !string.IsNullOrWhiteSpace(apiResponse.Dados.Token))
        {
            await _local.SetTokensAsync(apiResponse.Dados.Token, apiResponse.Dados.RefreshToken, ct);
            _local.SetIsAuthenticatedFlag(true);
            return true;
        }

        _local.SetIsAuthenticatedFlag(false);
        return false;
    }

    public async Task LogoutAsync(CancellationToken ct = default)
    {
        _local.ClearTokens();
        _local.SetIsAuthenticatedFlag(false);
        await _userRepository.ClearUserAsync();
    }
}
