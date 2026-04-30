using System.Security.Claims;
using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeuCatalogo.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/lancamentos/{lancamentoId:guid}/baixas")]
[Authorize]
public class LancamentoBaixasController : BaseApiController
{
    private readonly ILancamentoBaixaService _service;

    public LancamentoBaixasController(ILancamentoBaixaService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(Guid lancamentoId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.ListarAsync(lancamentoId, userId));
    }

    [HttpPost]
    public async Task<IActionResult> Adicionar(Guid lancamentoId, [FromBody] LancamentoBaixaRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblemResponse(ModelState);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.AdicionarAsync(lancamentoId, request, userId));
    }

    [HttpDelete("{baixaId:guid}")]
    public async Task<IActionResult> Remover(Guid lancamentoId, Guid baixaId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var resp = await _service.RemoverAsync(lancamentoId, baixaId, userId);
        return resp is { IsSuccess: true } ? DeletedResponse() : HandleApiResponse(resp);
    }
}
