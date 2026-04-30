using System.Security.Claims;
using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MeuCatalogo.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/lancamentos/{lancamentoId:guid}/baixas")]
[Authorize]
[SwaggerTag("Baixas (pagamentos parciais ou totais) de um lançamento.")]
public class LancamentoBaixasController : BaseApiController
{
    private readonly ILancamentoBaixaService _service;

    public LancamentoBaixasController(ILancamentoBaixaService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lista as baixas registradas para um lançamento.
    /// </summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Listar baixas")]
    [SwaggerResponse(200, "Baixas retornadas", typeof(IEnumerable<LancamentoBaixaResponse>))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Lançamento não encontrado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Listar(Guid lancamentoId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.ListarAsync(lancamentoId, userId));
    }

    /// <summary>
    /// Adiciona uma baixa parcial ou total. O Status do lançamento é recalculado: Pendente → Parcial → Pago.
    /// </summary>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Adicionar baixa",
        Description = "Soma das baixas não pode ultrapassar o Valor do lançamento. Quando soma == Valor, status vira Pago; caso contrário, Parcial."
    )]
    [SwaggerResponse(201, "Baixa criada", typeof(LancamentoBaixaResponse))]
    [SwaggerResponse(400, "Valor excede saldo em aberto ou dados inválidos", typeof(ProblemDetails))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Lançamento ou conta não encontrada", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Adicionar(Guid lancamentoId, [FromBody] LancamentoBaixaRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblemResponse(ModelState);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.AdicionarAsync(lancamentoId, request, userId));
    }

    /// <summary>
    /// Remove uma baixa e recalcula o status do lançamento.
    /// </summary>
    [HttpDelete("{baixaId:guid}")]
    [SwaggerOperation(Summary = "Remover baixa")]
    [SwaggerResponse(204, "Baixa removida")]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Baixa não encontrada", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Remover(Guid lancamentoId, Guid baixaId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var resp = await _service.RemoverAsync(lancamentoId, baixaId, userId);
        return resp is { IsSuccess: true } ? DeletedResponse() : HandleApiResponse(resp);
    }
}
