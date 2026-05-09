using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Serilog.Context;
using Sentry;

namespace MeuCatalogo.API.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    private const int MaxBodyCaptureBytes = 16 * 1024;

    private static readonly HashSet<string> FullMaskKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "senha", "token", "refreshToken", "refresh_token",
        "accessToken", "access_token", "secret", "authorization"
    };

    private static readonly HashSet<string> PartialMaskKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "cpf", "email"
    };

    public async Task Invoke(HttpContext context)
    {
        if (HasJsonBody(context.Request))
            context.Request.EnableBuffering();

        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        string correlationId = context.TraceIdentifier ?? Guid.NewGuid().ToString();

        context.Response.Headers["X-Correlation-ID"] = correlationId;

        string? requestBody = HasJsonBody(context.Request)
            ? await ReadAndMaskRequestBodyAsync(context.Request)
            : null;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            SentrySdk.ConfigureScope(scope =>
            {
                scope.SetTag("correlationId", correlationId);
                scope.SetTag("path", context.Request.Path);
                scope.SetTag("method", context.Request.Method);
                scope.SetExtra("request", new
                {
                    path = context.Request.Path,
                    method = context.Request.Method,
                    queryString = context.Request.QueryString.ToString(),
                    userAgent = context.Request.Headers.UserAgent.ToString(),
                    correlationId = correlationId
                });
                if (requestBody is not null)
                    scope.SetExtra("requestBody", requestBody);
            });
            SentrySdk.CaptureException(exception);

            _logger.LogError(exception, "Erro inesperado. CorrelationId: {CorrelationId}, Path: {Path}, Method: {Method}",
                correlationId, context.Request.Path, context.Request.Method);

             int statusCode;
            string title;
            string detail = exception.Message;

            ProblemDetails problem;

            switch (exception)
            {
                case UnauthorizedAccessException:
                    statusCode = (int)HttpStatusCode.Forbidden;
                    title = "Acesso não autorizado";
                    problem = new ProblemDetails
                    {
                        Status = statusCode,
                        Title = title,
                        Detail = detail,
                        Instance = context.Request.Path,
                        Type = $"https://httpstatuses.com/{statusCode}"
                    };
                    break;

                case ValidationException vex:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    title = "Erro de validação";

                    var validationErrors = vex.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()
                        );

                    problem = new ValidationProblemDetails(validationErrors)
                    {
                        Status = statusCode,
                        Title = title,
                        Detail = "Um ou mais erros de validação ocorreram.",
                        Instance = context.Request.Path,
                        Type = $"https://httpstatuses.com/{statusCode}"
                    };
                    break;

                default:
                    statusCode = (int)HttpStatusCode.InternalServerError;
                    title = "Erro interno no servidor";
                    detail = "Ocorreu um erro inesperado. Tente novamente mais tarde.";
                    problem = new ProblemDetails
                    {
                        Status = statusCode,
                        Title = title,
                        Detail = detail,
                        Instance = context.Request.Path,
                        Type = $"https://httpstatuses.com/{statusCode}"
                    };
                    break;
            }

            problem.Extensions["correlationId"] = correlationId;

            var payload = JsonSerializer.Serialize(problem);
            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = statusCode;

            _logger.LogError(exception, "Erro inesperado. CorrelationId: {CorrelationId}, Path: {Path}, Method: {Method}",
                correlationId, context.Request.Path, context.Request.Method);

            await context.Response.WriteAsync(payload);
        }
    }

    private static bool HasJsonBody(HttpRequest request)
    {
        if (request.ContentLength is null or 0) return false;
        var contentType = request.ContentType;
        return contentType is not null && contentType.Contains("json", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<string?> ReadAndMaskRequestBodyAsync(HttpRequest request)
    {
        try
        {
            if (!request.Body.CanSeek) return null;
            request.Body.Position = 0;

            var buffer = new byte[MaxBodyCaptureBytes];
            int read = await request.Body.ReadAsync(buffer);
            request.Body.Position = 0;

            if (read == 0) return null;

            var raw = System.Text.Encoding.UTF8.GetString(buffer, 0, read);
            var truncated = read == MaxBodyCaptureBytes;
            var masked = MaskJson(raw) ?? raw;
            return truncated ? masked + "...[truncated]" : masked;
        }
        catch
        {
            return null;
        }
    }

    private static string? MaskJson(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream))
            {
                WriteMasked(document.RootElement, writer);
            }
            return System.Text.Encoding.UTF8.GetString(stream.ToArray());
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static void WriteMasked(JsonElement element, Utf8JsonWriter writer)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var property in element.EnumerateObject())
                {
                    writer.WritePropertyName(property.Name);
                    if (FullMaskKeys.Contains(property.Name))
                        writer.WriteStringValue("***");
                    else if (PartialMaskKeys.Contains(property.Name) && property.Value.ValueKind == JsonValueKind.String)
                        writer.WriteStringValue(PartialMask(property.Value.GetString()));
                    else
                        WriteMasked(property.Value, writer);
                }
                writer.WriteEndObject();
                break;
            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in element.EnumerateArray())
                    WriteMasked(item, writer);
                writer.WriteEndArray();
                break;
            default:
                element.WriteTo(writer);
                break;
        }
    }

    private static string PartialMask(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "***";
        if (value.Length <= 6) return "***";
        return $"{value[..4]}***{value[^2..]}";
    }
}
