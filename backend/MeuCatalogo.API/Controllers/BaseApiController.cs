using MeuCatalogo.Application.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MeuCatalogo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected ActionResult ValidationProblemResponse(ModelStateDictionary modelState)
    {
        var errorDictionary = modelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        var problemDetails = new ValidationProblemDetails(errorDictionary)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Erros de validação",
            Detail = "Um ou mais campos estão inválidos.",
            Type = "https://httpstatuses.com/400",
            Instance = HttpContext?.Request?.Path
        };

        return BadRequest(problemDetails);
    }

    /// <summary>
    /// Retorna 200 OK com dados no corpo.
    /// </summary>
    protected ActionResult<T> OkResponse<T>(T data)
    {
        return Ok(data);
    }

    /// <summary>
    /// Retorna 201 Created com dados e location.
    /// </summary>
    protected ActionResult CreatedResponse<T>(string location, T data)
    {
        return Created(location, data);
    }

    /// <summary>
    /// Retorna 204 NoContent sem corpo.
    /// </summary>
    protected ActionResult NoContentResponse()
    {
        return NoContent();
    }

    /// <summary>
    /// Retorna 400/500/etc com erro genérico (sem tipo).
    /// </summary>
    protected ActionResult ErrorResponse(string message, int statusCode = 400, List<string>? errors = null)
    {
        var response = ApiResponse<object>.Error(message, errors);
        return StatusCode(statusCode, response);
    }

    /// <summary>
    /// Retorna 400/500/etc com erro tipado.
    /// </summary>
    protected ActionResult<T> ErrorResponse<T>(string title, string message, List<string>? errors = null)
    {
        var problemDetails = CreateProblemDetails(500, title, message);
        return new ObjectResult(problemDetails)
        {
            StatusCode = problemDetails.Status,
            ContentTypes = { "application/problem+json" }
        };
    }

    /// <summary>
    /// Retorna 200 OK após atualização com os dados atualizados.
    /// </summary>
    protected ActionResult<T> UpdatedResponse<T>(T data)
    {
        return Ok(data);
    }

    /// <summary>
    /// Retorna 204 NoContent após remoção com sucesso.
    /// </summary>
    protected ActionResult DeletedResponse()
    {
        return NoContent();
    }

    /// <summary>
    /// Retorna 404 Not Found com mensagem padronizada.
    /// </summary>
    protected ActionResult NotFoundResponse(string message = "Recurso não encontrado")
    {
        return NotFound(ApiResponse<object>.Error(message));
    }

    protected ActionResult BadRequestResponse(string message)
    {
        var problemDetails = CreateProblemDetails(400, "Requisição inválida", message);
        return new ObjectResult(problemDetails)
        {
            StatusCode = problemDetails.Status,
            ContentTypes = { "application/problem+json" }
        };
    }

    protected ActionResult BadRequestResponse<T>(string message)
    {
        var problemDetails = CreateProblemDetails(400, "Requisição inválida", message);
        return new ObjectResult(problemDetails)
        {
            StatusCode = problemDetails.Status,
            ContentTypes = { "application/problem+json" }
        };
    }

    /// <summary>
    /// Retorna 401 Unauthorized.
    /// </summary>
    protected ActionResult UnauthorizedResponse(string message = "Acesso não autorizado")
    {
        return Unauthorized(ApiResponse<object>.Error(message));
    }

    /// <summary>
    /// Retorna 403 Forbidden.
    /// </summary>
    protected ActionResult ForbiddenResponse(string message = "Acesso negado")
    {
        return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Error(message));
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
