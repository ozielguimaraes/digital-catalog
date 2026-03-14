using System.Text;
using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Auth.Data;
using MeuCatalogo.Features.Auth.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Auth.Data.Remote.Contracts.Responses;
using MeuCatalogo.Features.Auth.Validators;

namespace MeuCatalogo.Features.Auth.UseCases;

public sealed class SignupUseCase : IUseCase<SignupRequest, ApiResponse<UserResponse>>
{
    private readonly IAuthRepository _authRepository;

    public SignupUseCase(IAuthRepository authRepository)
    {
        _authRepository = authRepository;
    }

    public async Task<ApiResponse<UserResponse>> ExecuteAsync(SignupRequest request)
    {
        var validator = new SignupValidator(request);
        if (validator.IsValid)
        {
            return await _authRepository.SignupAsync(request);
        }

        var messages = validator.Notifications.Select(x => x.Message);
        var sb = new StringBuilder();
        foreach (string message in messages)
            sb.Append($"{message}\n");

        string error = sb.ToString().TrimEnd();
        return ApiResponse<UserResponse>.Error(error);
    }
}
