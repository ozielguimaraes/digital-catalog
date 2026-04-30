using System.Security.Claims;
using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeuCatalogo.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/relatorios-financeiros")]
[Authorize]
public class RelatoriosFinanceirosController : BaseApiController
{
    private readonly IRelatorioFinanceiroService _service;

    public RelatoriosFinanceirosController(IRelatorioFinanceiroService service)
    {
        _service = service;
    }

    [HttpPost("lancamentos-por-categoria")]
    public async Task<IActionResult> LancamentosPorCategoria([FromBody] RelatorioFinanceiroRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblemResponse(ModelState);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.LancamentosPorCategoriaAsync(request, userId));
    }
}
