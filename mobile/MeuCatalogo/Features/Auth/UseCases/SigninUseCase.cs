using System.Text;
using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Auth.Data;
using MeuCatalogo.Features.Auth.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Auth.Data.Remote.Contracts.Responses;
using MeuCatalogo.Features.Auth.Validators;
using MeuCatalogo.Infrastructure;

namespace MeuCatalogo.Features.Auth.UseCases;

public class SigninUseCase(IAuthRepository authRepository) : IUseCase<SigninRequest, ApiResponse<SigninResponse>>
{
    public async Task<ApiResponse<SigninResponse>> ExecuteAsync(SigninRequest request)
    {
        var validator = new SigninValidator(request);
        if (validator.IsValid)
        {
            return await authRepository.SigninAsync(request);
        }

        var messages = validator.Notifications.Select(x => x.Message);
        var sb = new StringBuilder();

        foreach (string message in messages)
            sb.Append($"{message}\n");

        string error = sb.ToString().TrimEnd();
        return ApiResponse<SigninResponse>.Error(error);
    }
}
