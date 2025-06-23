using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Serilog.Context;

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

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        string correlationId = context.TraceIdentifier ?? Guid.NewGuid().ToString();

        context.Response.Headers["X-Correlation-ID"] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
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

            return context.Response.WriteAsync(payload);
        }
    }
}
