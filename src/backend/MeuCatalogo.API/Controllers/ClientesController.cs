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
[SwaggerTag("Gerenciamento de clientes")]
public class ClientesController : BaseApiController
{
    private readonly IClienteService _clienteService;
    private readonly ILogger<ClientesController> _logger;

    public ClientesController(IClienteService clienteService, ILogger<ClientesController> logger)
    {
        _clienteService = clienteService;
        _logger = logger;
    }

    /// <summary>
    /// Lista todos os clientes.
    /// </summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Listar clientes",
        Description = "Retorna todos os clientes cadastrados."
    )]
    [SwaggerResponse(200, "Lista de clientes retornada com sucesso", typeof(IEnumerable<ClienteResponse>))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Obter()
    {
        _logger.LogInformation("Listando clientes");
        var response = await _clienteService.GetAllAsync();
        return HandleApiResponse(response);
    }

    /// <summary>
    /// Obtém um cliente pelo ID.
    /// </summary>
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Obter cliente por ID",
        Description = "Retorna os dados de um cliente específico."
    )]
    [SwaggerResponse(200, "Cliente encontrado", typeof(ClienteResponse))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Cliente não encontrado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Obter(Guid id)
    {
        _logger.LogInformation("Obtendo cliente {ClienteId}", id);
        var response = await _clienteService.GetByIdAsync(id);
        return HandleApiResponse(response);
    }

    /// <summary>
    /// Obtém um cliente pelo e-mail.
    /// </summary>
    [HttpGet("email/{email}")]
    [SwaggerOperation(
        Summary = "Obter cliente por e-mail",
        Description = "Retorna o cliente associado ao e-mail informado."
    )]
    [SwaggerResponse(200, "Cliente encontrado", typeof(ClienteResponse))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Cliente não encontrado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> ObterPorEmail(string email)
    {
        _logger.LogInformation("Obtendo cliente por e-mail {Email}", email);
        var response = await _clienteService.GetByEmailAsync(email);
        return HandleApiResponse(response);
    }

    /// <summary>
    /// Cria um novo cliente.
    /// </summary>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Criar cliente",
        Description = "Cadastra um novo cliente."
    )]
    [SwaggerResponse(201, "Cliente criado com sucesso", typeof(ClienteResponse))]
    [SwaggerResponse(400, "Dados inválidos fornecidos", typeof(ProblemDetails))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Adicionar([FromBody] ClienteRequest request)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState inválido ao criar cliente: {@ModelState}", ModelState);
            return ValidationProblemResponse(ModelState);
        }

        _logger.LogInformation("Criando cliente {Email}", request.Email);
        var response = await _clienteService.CreateAsync(request);
        return HandleApiResponse(response);
    }

    /// <summary>
    /// Atualiza dados de um cliente existente.
    /// </summary>
    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "Atualizar cliente",
        Description = "Atualiza os dados de um cliente já cadastrado."
    )]
    [SwaggerResponse(200, "Cliente atualizado com sucesso", typeof(ClienteResponse))]
    [SwaggerResponse(400, "Dados inválidos fornecidos", typeof(ProblemDetails))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Cliente não encontrado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] ClienteRequest request)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState inválido ao atualizar cliente: {@ModelState}", ModelState);
            return ValidationProblemResponse(ModelState);
        }

        _logger.LogInformation("Atualizando cliente {ClienteId}", id);
        var response = await _clienteService.UpdateAsync(id, request);
        return HandleApiResponse(response);
    }

    /// <summary>
    /// Remove um cliente.
    /// </summary>
    [HttpDelete("{id}")]
    [SwaggerOperation(
        Summary = "Remover cliente",
        Description = "Remove um cliente sem pedidos associados."
    )]
    [SwaggerResponse(204, "Cliente removido com sucesso")]
    [SwaggerResponse(400, "Cliente possui pedidos e não pode ser removido", typeof(ProblemDetails))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Cliente não encontrado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Remover(Guid id)
    {
        _logger.LogInformation("Removendo cliente {ClienteId}", id);
        var response = await _clienteService.DeleteAsync(id);
        return response is { IsSuccess: true, Data: true } ? DeletedResponse() : HandleApiResponse(response);
    }
}
