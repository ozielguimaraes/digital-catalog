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
        if (string.IsNullOrWhiteSpace(content))
            return GetProblemDetailsForStatusCode(apiEx.StatusCode);

        try
        {
            return JsonSerializer.Deserialize<ProblemDetails>(content, JsonOptions);
        }
        catch (JsonException)
        {
            // O backend pode devolver `errors` em formatos incompatíveis com
            // Refit.ProblemDetails (ex.: array de strings em vez de
            // Dictionary<string, string[]>). Como só consumimos title/detail/status,
            // fazemos parse defensivo desses campos.
            return TryParseProblemDetailsLoose(content) ?? GetProblemDetailsForStatusCode(apiEx.StatusCode);
        }
    }

    private static ProblemDetails? TryParseProblemDetailsLoose(string content)
    {
        try
        {
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return null;

            var problem = new ProblemDetails();
            if (root.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
                problem.Title = title.GetString();
            if (root.TryGetProperty("detail", out var detail) && detail.ValueKind == JsonValueKind.String)
                problem.Detail = detail.GetString();
            if (root.TryGetProperty("status", out var status) && status.ValueKind == JsonValueKind.Number && status.TryGetInt32(out var statusValue))
                problem.Status = statusValue;
            return problem;
        }
        catch (JsonException)
        {
            return null;
        }
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
