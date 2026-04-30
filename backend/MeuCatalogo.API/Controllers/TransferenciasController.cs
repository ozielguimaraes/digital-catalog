using System.Security.Claims;
using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MeuCatalogo.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[SwaggerTag("Transferências entre contas e pagamento de fatura (gera dois lançamentos vinculados).")]
public class TransferenciasController : BaseApiController
{
    private readonly ITransferenciaService _service;

    public TransferenciasController(ITransferenciaService service)
    {
        _service = service;
    }

    /// <summary>
    /// Cria uma transferência entre duas contas. Gera dois lançamentos com Operacao=Transferencia, vinculados via LancamentoTransferenciaId.
    /// </summary>
    [HttpPost("entre-contas")]
    [SwaggerOperation(
        Summary = "Transferência entre contas",
        Description = "Conta de origem e destino devem ser diferentes; cartão de crédito não pode ser origem em transferência entre contas."
    )]
    [SwaggerResponse(201, "Transferência criada", typeof(LancamentoResponse))]
    [SwaggerResponse(400, "Origem == destino, conta inválida ou dados ausentes", typeof(ProblemDetails))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Conta não encontrada", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> EntreContas([FromBody] TransferenciaRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblemResponse(ModelState);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.CriarEntreContasAsync(request, userId));
    }

    /// <summary>
    /// Cria um pagamento de fatura — transferência de uma conta comum para um cartão.
    /// </summary>
    [HttpPost("pagamento-fatura")]
    [SwaggerOperation(
        Summary = "Pagamento de fatura",
        Description = "Origem deve ser conta não-cartão; destino deve ser conta de cartão de crédito. Atualiza ValorPago da fatura correspondente."
    )]
    [SwaggerResponse(201, "Pagamento registrado", typeof(LancamentoResponse))]
    [SwaggerResponse(400, "Combinação inválida de contas", typeof(ProblemDetails))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Fatura ou conta não encontrada", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> PagamentoFatura([FromBody] TransferenciaRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblemResponse(ModelState);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.CriarPagamentoFaturaAsync(request, userId));
    }

    /// <summary>
    /// Exclui uma transferência removendo os dois lançamentos vinculados.
    /// </summary>
    [HttpDelete("{lancamentoId:guid}")]
    [SwaggerOperation(Summary = "Excluir transferência")]
    [SwaggerResponse(204, "Transferência removida")]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Lançamento não encontrado ou não é transferência", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Remover(Guid lancamentoId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var resp = await _service.ExcluirAsync(lancamentoId, userId);
        return resp is { IsSuccess: true } ? DeletedResponse() : HandleApiResponse(resp);
    }
}
