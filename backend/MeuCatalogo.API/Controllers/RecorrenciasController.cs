using System.Security.Claims;
using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MeuCatalogo.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[SwaggerTag("Recorrências mensais/semanais/anuais (luz, água, internet, salário) com geração on-demand.")]
public class RecorrenciasController : BaseApiController
{
    private readonly IRecorrenciaService _service;

    public RecorrenciasController(IRecorrenciaService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lista todas as recorrências do usuário.
    /// </summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Listar recorrências")]
    [SwaggerResponse(200, "Recorrências retornadas", typeof(IEnumerable<RecorrenciaResponse>))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Listar()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.GetAllAsync(userId));
    }

    /// <summary>
    /// Obtém uma recorrência pelo ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Obter recorrência por ID")]
    [SwaggerResponse(200, "Recorrência retornada", typeof(RecorrenciaResponse))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Recorrência não encontrada", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Obter(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.GetByIdAsync(id, userId));
    }

    /// <summary>
    /// Cria uma recorrência. Use ValorPadrao=null para recorrências de valor variável (ex.: conta de luz).
    /// </summary>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Criar recorrência",
        Description = "ValorPadrao opcional: quando ausente, os lançamentos materializados nascem com Valor=0 e Realizado=false (para o usuário ajustar)."
    )]
    [SwaggerResponse(201, "Recorrência criada", typeof(RecorrenciaResponse))]
    [SwaggerResponse(400, "Dados inválidos", typeof(ProblemDetails))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Adicionar([FromBody] RecorrenciaRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblemResponse(ModelState);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.CreateAsync(request, userId));
    }

    /// <summary>
    /// Atualiza uma recorrência.
    /// </summary>
    [HttpPut("{id:guid}")]
    [SwaggerOperation(Summary = "Atualizar recorrência")]
    [SwaggerResponse(200, "Recorrência atualizada", typeof(RecorrenciaResponse))]
    [SwaggerResponse(400, "Dados inválidos", typeof(ProblemDetails))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Recorrência não encontrada", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] RecorrenciaRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblemResponse(ModelState);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.UpdateAsync(id, request, userId));
    }

    /// <summary>
    /// Remove uma recorrência (não toca em lançamentos já materializados).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Remover recorrência")]
    [SwaggerResponse(204, "Recorrência removida")]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Recorrência não encontrada", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Remover(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var resp = await _service.DeleteAsync(id, userId);
        return resp is { IsSuccess: true } ? DeletedResponse() : HandleApiResponse(resp);
    }

    /// <summary>
    /// Reativa uma recorrência inativa.
    /// </summary>
    [HttpPost("{id:guid}/ativar")]
    [SwaggerOperation(Summary = "Ativar recorrência")]
    [SwaggerResponse(200, "Recorrência ativada", typeof(RecorrenciaResponse))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Recorrência não encontrada", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Ativar(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.SetAtivoAsync(id, userId, true));
    }

    /// <summary>
    /// Inativa uma recorrência (interrompe a materialização sem apagá-la).
    /// </summary>
    [HttpPost("{id:guid}/inativar")]
    [SwaggerOperation(Summary = "Inativar recorrência")]
    [SwaggerResponse(200, "Recorrência inativada", typeof(RecorrenciaResponse))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Recorrência não encontrada", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Inativar(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.SetAtivoAsync(id, userId, false));
    }
}
