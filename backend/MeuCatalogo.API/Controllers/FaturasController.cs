using System.Security.Claims;
using MeuCatalogo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeuCatalogo.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class FaturasController : BaseApiController
{
    private readonly IFaturaService _service;

    public FaturasController(IFaturaService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Obter([FromQuery] Guid contaId, [FromQuery] int mes, [FromQuery] int ano)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.GetAsync(contaId, mes, ano, userId));
    }

    [HttpGet("conta/{contaId:guid}")]
    public async Task<IActionResult> ListarPorConta(Guid contaId, [FromQuery] int? ano)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.ListarPorContaAsync(contaId, userId, ano));
    }
}
