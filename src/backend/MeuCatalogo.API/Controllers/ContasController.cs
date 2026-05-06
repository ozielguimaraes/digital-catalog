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
[SwaggerTag("Contas financeiras (carteira, conta corrente, cartão de crédito, poupança).")]
public class ContasController : BaseApiController
{
    private readonly IContaService _service;

    public ContasController(IContaService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lista as contas do usuário autenticado.
    /// </summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Listar contas",
        Description = "Retorna as contas do usuário. Por padrão omite as inativas; use incluirInativas=true para trazer todas."
    )]
    [SwaggerResponse(200, "Lista de contas retornada com sucesso", typeof(IEnumerable<ContaResponse>))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Listar([FromQuery] bool incluirInativas = false)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.GetAllAsync(userId, incluirInativas));
    }

    /// <summary>
    /// Obtém uma conta pelo ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Obter conta por ID")]
    [SwaggerResponse(200, "Conta encontrada", typeof(ContaResponse))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Conta não encontrada", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Obter(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.GetByIdAsync(id, userId));
    }

    /// <summary>
    /// Cria uma nova conta.
    /// </summary>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Criar conta",
        Description = "Cria uma conta. Para Tipo=CartaoCredito, DiaFechamento e DiaVencimento são obrigatórios (1 a 31). SaldoInicial é ignorado em cartões."
    )]
    [SwaggerResponse(201, "Conta criada com sucesso", typeof(ContaResponse))]
    [SwaggerResponse(400, "Dados inválidos", typeof(ProblemDetails))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Adicionar([FromBody] ContaRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblemResponse(ModelState);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.CreateAsync(request, userId));
    }

    /// <summary>
    /// Atualiza uma conta. Em cartões, alterar DiaFechamento/DiaVencimento recalcula faturas futuras não pagas.
    /// </summary>
    [HttpPut("{id:guid}")]
    [SwaggerOperation(
        Summary = "Atualizar conta",
        Description = "Atualiza dados da conta. Para cartão de crédito, alterar DiaFechamento ou DiaVencimento dispara o recálculo de DataInicio/DataFim/DataVencimento das faturas futuras não pagas."
    )]
    [SwaggerResponse(200, "Conta atualizada", typeof(ContaResponse))]
    [SwaggerResponse(400, "Dados inválidos", typeof(ProblemDetails))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Conta não encontrada", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] ContaRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblemResponse(ModelState);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.UpdateAsync(id, request, userId));
    }

    /// <summary>
    /// Remove uma conta.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(
        Summary = "Remover conta",
        Description = "Remove a conta. Falha se houver lançamentos vinculados; nesse caso prefira inativar."
    )]
    [SwaggerResponse(204, "Conta removida")]
    [SwaggerResponse(400, "Conta possui lançamentos e não pode ser removida", typeof(ProblemDetails))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Conta não encontrada", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Remover(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var resp = await _service.DeleteAsync(id, userId);
        return resp is { IsSuccess: true } ? DeletedResponse() : HandleApiResponse(resp);
    }

    /// <summary>
    /// Ativa uma conta inativada.
    /// </summary>
    [HttpPost("{id:guid}/ativar")]
    [SwaggerOperation(Summary = "Ativar conta")]
    [SwaggerResponse(200, "Conta ativada", typeof(ContaResponse))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Conta não encontrada", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Ativar(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.SetAtivoAsync(id, userId, true));
    }

    /// <summary>
    /// Inativa uma conta sem removê-la.
    /// </summary>
    [HttpPost("{id:guid}/inativar")]
    [SwaggerOperation(Summary = "Inativar conta")]
    [SwaggerResponse(200, "Conta inativada", typeof(ContaResponse))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Conta não encontrada", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Inativar(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.SetAtivoAsync(id, userId, false));
    }
}
