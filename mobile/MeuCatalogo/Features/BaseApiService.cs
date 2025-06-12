using System.Text.Json;
using Refit;

namespace MeuCatalogo.Features;

public abstract class BaseApiService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected const string TokenKey = "auth_token";
    protected const string UserInfoKey = "user_info_key";

    protected static string ObterMensagemErroApi(ApiException apiEx)
    {
        string? content = apiEx.Content;
        if (string.IsNullOrWhiteSpace(content)) return "Erro desconhecido da API.";

        var problem = GetProblemDetails(apiEx);
        return problem?.Detail ?? problem?.Title ?? "Erro desconhecido da API.";
    }

    protected static ProblemDetails? GetProblemDetails(ApiException apiEx)
    {
        string? content = apiEx.Content;
        return string.IsNullOrWhiteSpace(content) ? null : JsonSerializer.Deserialize<ProblemDetails>(content, JsonOptions);
    }
}
