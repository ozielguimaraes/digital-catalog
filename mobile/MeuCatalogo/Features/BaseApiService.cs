using System.Net;
using System.Text.Json;
using Refit;

namespace MeuCatalogo.Features;

public abstract class BaseApiService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public const string TokenKey = "auth_token";
    public const string RefreshTokenKey = "auth_refresh_token";


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
        return string.IsNullOrWhiteSpace(content) ? GetProblemDetailsForStatusCode(apiEx.StatusCode) : JsonSerializer.Deserialize<ProblemDetails>(content, JsonOptions);
    }

    private static ProblemDetails GetProblemDetailsForStatusCode(HttpStatusCode statusCode)
    {
        return new ProblemDetails
        {
            Status = (int)statusCode,
            Title = "Erro na requisição",
            Detail = statusCode switch
            {
                HttpStatusCode.BadRequest => "A requisição foi inválida.",
                HttpStatusCode.Unauthorized => "Você não tem autorização para acessar este recurso.",
                HttpStatusCode.Forbidden => "Acesso proibido ao recurso.",
                HttpStatusCode.NotFound => "O recurso solicitado não foi encontrado.",
                HttpStatusCode.InternalServerError => "Ocorreu um erro interno no servidor.",
                _ => "Ocorreu um erro inesperado ao processar a requisição."
            }
        };
    }

    protected static async Task<string> ObterBearerTokenAsync()
    {
        string? bearerToken = await SecureStorage.Default.GetAsync(TokenKey);
        return $"Bearer {bearerToken ?? string.Empty}";
    }
}
