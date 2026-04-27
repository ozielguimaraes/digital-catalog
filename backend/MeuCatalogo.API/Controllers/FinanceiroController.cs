using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Swashbuckle.AspNetCore.Annotations;

namespace MeuCatalogo.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[SwaggerTag("Lançamentos financeiros (a receber e a pagar)")]
public class FinanceiroController : BaseApiController
{
    private readonly IFinanceiroService _financeiroService;
    private readonly ILogger<FinanceiroController> _logger;

    public FinanceiroController(IFinanceiroService financeiroService, ILogger<FinanceiroController> logger)
    {
        _financeiroService = financeiroService;
        _logger = logger;
    }

    [HttpGet("resumo")]
    [SwaggerOperation(Summary = "Obter resumo financeiro", Description = "Totais a receber, a pagar e movimentações do mês corrente.")]
    [SwaggerResponse(200, "Resumo gerado", typeof(FinanceiroResumoResponse))]
    public async Task<IActionResult> Resumo()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var response = await _financeiroService.GetResumoAsync(userId);
        return HandleApiResponse(response);
    }

    [HttpGet("lancamentos")]
    [SwaggerOperation(Summary = "Listar lançamentos", Description = "Filtro opcional por tipo (Receber/Pagar).")]
    [SwaggerResponse(200, "Lista retornada", typeof(IEnumerable<LancamentoResponse>))]
    public async Task<IActionResult> Lancamentos([FromQuery] LancamentoTipo? tipo)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var response = await _financeiroService.GetAllAsync(userId, tipo);
        return HandleApiResponse(response);
    }

    [HttpGet("lancamentos/{id}")]
    [SwaggerOperation(Summary = "Obter lançamento por ID")]
    [SwaggerResponse(200, "Encontrado", typeof(LancamentoResponse))]
    [SwaggerResponse(404, "Não encontrado", typeof(ProblemDetails))]
    public async Task<IActionResult> Obter(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var response = await _financeiroService.GetByIdAsync(id, userId);
        return HandleApiResponse(response);
    }

    [HttpPost("lancamentos")]
    [SwaggerOperation(Summary = "Criar lançamento")]
    [SwaggerResponse(201, "Criado", typeof(LancamentoResponse))]
    [SwaggerResponse(400, "Dados inválidos", typeof(ProblemDetails))]
    public async Task<IActionResult> Adicionar([FromBody] LancamentoRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblemResponse(ModelState);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var response = await _financeiroService.CreateAsync(request, userId);
        return HandleApiResponse(response);
    }

    [HttpPut("lancamentos/{id}")]
    [SwaggerOperation(Summary = "Atualizar lançamento")]
    [SwaggerResponse(200, "Atualizado", typeof(LancamentoResponse))]
    [SwaggerResponse(404, "Não encontrado", typeof(ProblemDetails))]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] LancamentoRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblemResponse(ModelState);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var response = await _financeiroService.UpdateAsync(id, request, userId);
        return HandleApiResponse(response);
    }

    [HttpDelete("lancamentos/{id}")]
    [SwaggerOperation(Summary = "Remover lançamento")]
    [SwaggerResponse(204, "Removido")]
    [SwaggerResponse(404, "Não encontrado", typeof(ProblemDetails))]
    public async Task<IActionResult> Remover(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var response = await _financeiroService.DeleteAsync(id, userId);
        return response is { IsSuccess: true, Data: true } ? DeletedResponse() : HandleApiResponse(response);
    }
}
