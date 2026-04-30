using System.Security.Claims;
using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeuCatalogo.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class TransferenciasController : BaseApiController
{
    private readonly ITransferenciaService _service;

    public TransferenciasController(ITransferenciaService service)
    {
        _service = service;
    }

    [HttpPost("entre-contas")]
    public async Task<IActionResult> EntreContas([FromBody] TransferenciaRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblemResponse(ModelState);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.CriarEntreContasAsync(request, userId));
    }

    [HttpPost("pagamento-fatura")]
    public async Task<IActionResult> PagamentoFatura([FromBody] TransferenciaRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblemResponse(ModelState);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.CriarPagamentoFaturaAsync(request, userId));
    }

    [HttpDelete("{lancamentoId:guid}")]
    public async Task<IActionResult> Remover(Guid lancamentoId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var resp = await _service.ExcluirAsync(lancamentoId, userId);
        return resp is { IsSuccess: true } ? DeletedResponse() : HandleApiResponse(resp);
    }
}
