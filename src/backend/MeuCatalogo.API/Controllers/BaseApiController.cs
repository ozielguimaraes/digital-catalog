using System.Net;
using Asp.Versioning;
using MeuCatalogo.Application.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MeuCatalogo.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected IActionResult OkResponse<T>(T data) => Ok(data);
    protected ActionResult NoContentResponse() => NoContent();
    protected IActionResult CreatedResponse<T>(T data) => StatusCode(StatusCodes.Status201Created, data);
    protected ActionResult<T> UpdatedResponse<T>(T data) => Ok(data);
    protected ActionResult DeletedResponse() => NoContent();

    protected ActionResult ValidationProblemResponse(ModelStateDictionary modelState)
    {
        var errorDictionary = modelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

        return ProblemResponse(StatusCodes.Status400BadRequest,
            "Erros de validação",
            "Um ou mais campos estão inválidos.",
            errorDictionary);
    }

    protected ActionResult BadRequestResponse(string detail,
        IEnumerable<string>? errors = null)
        => ProblemResponse(StatusCodes.Status400BadRequest,
            "Requisição inválida",
            detail,
            NormalizeErrors(errors));

    protected ActionResult NotFoundResponse(string detail = "Recurso não encontrado")
        => ProblemResponse(StatusCodes.Status404NotFound,
            "Não encontrado",
            detail);

    protected ActionResult UnauthorizedResponse(string detail = "Acesso não autorizado")
        => ProblemResponse(StatusCodes.Status401Unauthorized,
            "Não autorizado",
            detail);

    protected ActionResult ForbiddenResponse(string detail = "Acesso negado")
        => ProblemResponse(StatusCodes.Status403Forbidden,
            "Acesso negado",
            detail);

    protected ActionResult ProblemResponse(int statusCode,
        string title,
        string detail,
        IDictionary<string, string[]>? errors = null)
    {
        var problem = CreateProblemDetails(statusCode, title, detail);

        if (errors is not null && errors.Count > 0)
            problem.Extensions["errors"] = errors;

        return new ObjectResult(problem) { StatusCode = statusCode, ContentTypes = { "application/problem+json" } };
    }

    private static IDictionary<string, string[]>? NormalizeErrors(IEnumerable<string>? errors)
    {
        if (errors is null) return null;
        var array = errors.Where(e => !string.IsNullOrWhiteSpace(e)).ToArray();
        return array.Length == 0 ? null : new Dictionary<string, string[]> { [""] = array };
    }

    protected IActionResult HandleApiResponse<T>(ApiResponse<T> response)
    {
        if (response.IsSuccess)
        {
            return response.Type switch
            {
                ResponseType.Created => CreatedResponse(response.Data),
                ResponseType.Success => OkResponse(response.Data),
                ResponseType.Deleted => DeletedResponse(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        return response.Type switch
        {
            ResponseType.Validation => BadRequestResponse(
                response.Message ?? "Erro de validação",
                response.Errors),
            ResponseType.Forbidden => ForbiddenResponse(response.Message ?? "Acesso negado"),
            ResponseType.NotFound => NotFoundResponse(response.Message ?? "Recurso não encontrado"),
            _ => ProblemResponse(StatusCodes.Status400BadRequest,
                "Erro",
                response.Message ?? "Erro na operação",
                NormalizeErrors(response.Errors))
        };
    }

    private ProblemDetails CreateProblemDetails(int statusCode, string title, string detail)
    {
        string correlationId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();
        HttpContext?.Response?.Headers.TryAdd("X-Correlation-ID", correlationId);

        return new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = HttpContext?.Request?.Path,
            Type = $"https://httpstatuses.com/{statusCode}",
            Extensions = { ["correlationId"] = correlationId }
        };
    }
}
