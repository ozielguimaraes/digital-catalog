using System.Security.Claims;
using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MeuCatalogo.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/relatorios-financeiros")]
[Authorize]
[SwaggerTag("Relatórios financeiros agregados (regimes caixa e competência).")]
public class RelatoriosFinanceirosController : BaseApiController
{
    private readonly IRelatorioFinanceiroService _service;

    public RelatoriosFinanceirosController(IRelatorioFinanceiroService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lançamentos agrupados por categoria, com filtros e regime caixa/competência.
    /// </summary>
    [HttpPost("lancamentos-por-categoria")]
    [SwaggerOperation(
        Summary = "Lançamentos por categoria",
        Description = "Regime Competencia: agrupa por DataVencimento. Regime Caixa: cartão usa Fatura.DataVencimento; demais contas usam a data das baixas (ou DataPagamento quando Realizado=true sem baixa)."
    )]
    [SwaggerResponse(200, "Relatório retornado", typeof(RelatorioFinanceiroResponse))]
    [SwaggerResponse(400, "Filtros inválidos", typeof(ProblemDetails))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> LancamentosPorCategoria([FromBody] RelatorioFinanceiroRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblemResponse(ModelState);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.LancamentosPorCategoriaAsync(request, userId));
    }
}
