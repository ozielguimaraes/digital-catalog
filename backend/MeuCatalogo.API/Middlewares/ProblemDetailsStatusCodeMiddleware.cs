using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace MeuCatalogo.API.Middlewares;

public class ProblemDetailsStatusCodeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ProblemDetailsStatusCodeMiddleware> _logger;

    public ProblemDetailsStatusCodeMiddleware(RequestDelegate next, ILogger<ProblemDetailsStatusCodeMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        await _next(context);

        if (context.Response.HasStarted)
            return;

        if (context.Response.StatusCode >= 400 && !context.Response.ContentType?.Contains("problem+json") == true)
        {
            int statusCode = context.Response.StatusCode;
            string title = statusCode switch
            {
                400 => "Requisição inválida",
                401 => "Não autenticado",
                403 => "Acesso negado",
                404 => "Recurso não encontrado",
                _ => "Erro"
            };

            string correlationId = context.TraceIdentifier ?? Guid.NewGuid().ToString();
            context.Response.Headers["X-Correlation-ID"] = correlationId;

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = $"Erro ao processar a requisição. Código: {statusCode}",
                Instance = context.Request.Path,
                Type = $"https://httpstatuses.com/{statusCode}",
                Extensions = { ["correlationId"] = correlationId }
            };

            var payload = JsonSerializer.Serialize(problemDetails);
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsync(payload);
        }
    }
}
