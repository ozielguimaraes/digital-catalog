using System.Security.Claims;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MeuCatalogo.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/contas")]
[Authorize]
[SwaggerTag("Extrato de conta com saldo corrente e fechamento diário.")]
public class ExtratoController : BaseApiController
{
    private readonly IExtratoService _service;

    public ExtratoController(IExtratoService service)
    {
        _service = service;
    }

    /// <summary>
    /// Extrato de uma conta específica (movimentos + saldo inicial/final + fechamento diário).
    /// </summary>
    [HttpGet("{contaId:guid}/extrato")]
    [SwaggerOperation(
        Summary = "Extrato por conta",
        Description = "Retorna movimentos do período e saldos diários. Não funciona para cartão de crédito — para cartão, use o endpoint de faturas."
    )]
    [SwaggerResponse(200, "Extrato retornado", typeof(ExtratoResponse))]
    [SwaggerResponse(400, "Datas inválidas ou conta é cartão de crédito", typeof(ProblemDetails))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Conta não encontrada", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> ExtratoPorConta(Guid contaId, [FromQuery] DateTime dataInicio, [FromQuery] DateTime dataFim)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.ObterPorContaAsync(contaId, dataInicio, dataFim, userId));
    }

    /// <summary>
    /// Extrato consolidado de várias contas no mesmo período.
    /// </summary>
    [HttpGet("extrato")]
    [SwaggerOperation(
        Summary = "Extrato consolidado",
        Description = "Retorna o extrato consolidado das contas informadas (ou de todas as contas não-cartão do usuário, se contaIds vazio)."
    )]
    [SwaggerResponse(200, "Extrato consolidado retornado", typeof(ExtratoResponse))]
    [SwaggerResponse(400, "Datas inválidas", typeof(ProblemDetails))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> ExtratoConsolidado(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        [FromQuery] List<Guid>? contaIds = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.ObterConsolidadoAsync(dataInicio, dataFim, contaIds, userId));
    }
}
