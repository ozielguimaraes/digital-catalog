using System.Security.Claims;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MeuCatalogo.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[SwaggerTag("Faturas de cartão de crédito (mês/ano + lançamentos vinculados).")]
public class FaturasController : BaseApiController
{
    private readonly IFaturaService _service;

    public FaturasController(IFaturaService service)
    {
        _service = service;
    }

    /// <summary>
    /// Obtém a fatura de um cartão para o mês/ano especificado (cria se não existir).
    /// </summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Obter fatura por mês/ano",
        Description = "Retorna a fatura do mês/ano para a conta (cartão). Se não existir ainda, cria com base em DiaFechamento/DiaVencimento da conta."
    )]
    [SwaggerResponse(200, "Fatura retornada", typeof(FaturaResponse))]
    [SwaggerResponse(400, "Conta não é cartão de crédito ou parâmetros inválidos", typeof(ProblemDetails))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Conta não encontrada", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Obter([FromQuery] Guid contaId, [FromQuery] int mes, [FromQuery] int ano)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.GetAsync(contaId, mes, ano, userId));
    }

    /// <summary>
    /// Lista as faturas de uma conta (cartão), opcionalmente filtradas por ano.
    /// </summary>
    [HttpGet("conta/{contaId:guid}")]
    [SwaggerOperation(Summary = "Listar faturas por conta")]
    [SwaggerResponse(200, "Faturas retornadas", typeof(IEnumerable<FaturaResponse>))]
    [SwaggerResponse(400, "Conta não é cartão de crédito", typeof(ProblemDetails))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Conta não encontrada", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> ListarPorConta(Guid contaId, [FromQuery] int? ano)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.ListarPorContaAsync(contaId, userId, ano));
    }
}
