using System.Security.Claims;
using MeuCatalogo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeuCatalogo.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/contas")]
[Authorize]
public class ExtratoController : BaseApiController
{
    private readonly IExtratoService _service;

    public ExtratoController(IExtratoService service)
    {
        _service = service;
    }

    [HttpGet("{contaId:guid}/extrato")]
    public async Task<IActionResult> ExtratoPorConta(Guid contaId, [FromQuery] DateTime dataInicio, [FromQuery] DateTime dataFim)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.ObterPorContaAsync(contaId, dataInicio, dataFim, userId));
    }

    [HttpGet("extrato")]
    public async Task<IActionResult> ExtratoConsolidado(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        [FromQuery] List<Guid>? contaIds = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.ObterConsolidadoAsync(dataInicio, dataFim, contaIds, userId));
    }
}
