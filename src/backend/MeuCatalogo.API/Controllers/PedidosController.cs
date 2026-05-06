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
[SwaggerTag("Gerenciamento de pedidos")]
public class PedidosController : BaseApiController
{
    private readonly IPedidoService _pedidoService;
    private readonly ILogger<PedidosController> _logger;

    public PedidosController(IPedidoService pedidoService, ILogger<PedidosController> logger)
    {
        _pedidoService = pedidoService;
        _logger = logger;
    }

    /// <summary>
    /// Lista todos os pedidos.
    /// </summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Listar pedidos",
        Description = "Retorna todos os pedidos cadastrados."
    )]
    [SwaggerResponse(200, "Lista de pedidos retornada com sucesso", typeof(IEnumerable<PedidoResponse>))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Obter()
    {
        _logger.LogInformation("Listando pedidos");
        var response = await _pedidoService.GetAllAsync();
        return HandleApiResponse(response);
    }

    /// <summary>
    /// Obtém um pedido pelo ID.
    /// </summary>
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Obter pedido por ID",
        Description = "Retorna os dados completos de um pedido específico."
    )]
    [SwaggerResponse(200, "Pedido encontrado", typeof(PedidoResponse))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Pedido não encontrado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Obter(Guid id)
    {
        _logger.LogInformation("Obtendo pedido {PedidoId}", id);
        var response = await _pedidoService.GetByIdAsync(id);
        return HandleApiResponse(response);
    }

    /// <summary>
    /// Lista pedidos de um cliente específico.
    /// </summary>
    [HttpGet("cliente/{clienteId}")]
    [SwaggerOperation(
        Summary = "Listar pedidos por cliente",
        Description = "Retorna todos os pedidos associados a um cliente."
    )]
    [SwaggerResponse(200, "Lista de pedidos retornada", typeof(IEnumerable<PedidoResponse>))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> ObterPorCliente(Guid clienteId)
    {
        _logger.LogInformation("Listando pedidos do cliente {ClienteId}", clienteId);
        var response = await _pedidoService.GetByClienteAsync(clienteId);
        return HandleApiResponse(response);
    }

    /// <summary>
    /// Cria um novo pedido.
    /// </summary>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Criar pedido",
        Description = "Cria um novo pedido e debita o estoque correspondente."
    )]
    [SwaggerResponse(201, "Pedido criado com sucesso", typeof(PedidoResponse))]
    [SwaggerResponse(400, "Dados inválidos fornecidos", typeof(ProblemDetails))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Adicionar([FromBody] PedidoRequest request)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState inválido ao criar pedido: {@ModelState}", ModelState);
            return ValidationProblemResponse(ModelState);
        }

        _logger.LogInformation("Criando pedido para o cliente {ClienteId}", request.ClienteId);
        var response = await _pedidoService.CreateAsync(request);
        return HandleApiResponse(response);
    }

    /// <summary>
    /// Remove um pedido e devolve os itens ao estoque.
    /// </summary>
    [HttpDelete("{id}")]
    [SwaggerOperation(
        Summary = "Remover pedido",
        Description = "Remove um pedido e devolve os itens ao estoque (quando aplicável)."
    )]
    [SwaggerResponse(204, "Pedido removido com sucesso")]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Pedido não encontrado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Remover(Guid id)
    {
        _logger.LogInformation("Removendo pedido {PedidoId}", id);
        var response = await _pedidoService.DeleteAsync(id);
        return response is { IsSuccess: true, Data: true } ? DeletedResponse() : HandleApiResponse(response);
    }
}
