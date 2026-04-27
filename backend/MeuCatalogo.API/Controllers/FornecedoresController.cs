using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Swashbuckle.AspNetCore.Annotations;

namespace MeuCatalogo.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[SwaggerTag("Gerenciamento de fornecedores")]
public class FornecedoresController : BaseApiController
{
    private readonly IFornecedorService _fornecedorService;
    private readonly ILogger<FornecedoresController> _logger;

    public FornecedoresController(IFornecedorService fornecedorService, ILogger<FornecedoresController> logger)
    {
        _fornecedorService = fornecedorService;
        _logger = logger;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Listar fornecedores", Description = "Retorna fornecedores do usuário autenticado.")]
    [SwaggerResponse(200, "Lista retornada", typeof(IEnumerable<FornecedorResponse>))]
    [SwaggerResponse(401, "Não autenticado", typeof(ProblemDetails))]
    public async Task<IActionResult> Obter()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        _logger.LogInformation("Listando fornecedores do usuário {UserId}", userId);
        var response = await _fornecedorService.GetAllAsync(userId);
        return HandleApiResponse(response);
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Obter fornecedor por ID")]
    [SwaggerResponse(200, "Encontrado", typeof(FornecedorResponse))]
    [SwaggerResponse(404, "Não encontrado", typeof(ProblemDetails))]
    public async Task<IActionResult> Obter(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var response = await _fornecedorService.GetByIdAsync(id, userId);
        return HandleApiResponse(response);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Criar fornecedor")]
    [SwaggerResponse(201, "Criado", typeof(FornecedorResponse))]
    [SwaggerResponse(400, "Dados inválidos", typeof(ProblemDetails))]
    public async Task<IActionResult> Adicionar([FromBody] FornecedorRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblemResponse(ModelState);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        _logger.LogInformation("Criando fornecedor para usuário {UserId}", userId);
        var response = await _fornecedorService.CreateAsync(request, userId);
        return HandleApiResponse(response);
    }

    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Atualizar fornecedor")]
    [SwaggerResponse(200, "Atualizado", typeof(FornecedorResponse))]
    [SwaggerResponse(404, "Não encontrado", typeof(ProblemDetails))]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] FornecedorRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblemResponse(ModelState);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var response = await _fornecedorService.UpdateAsync(id, request, userId);
        return HandleApiResponse(response);
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Remover fornecedor")]
    [SwaggerResponse(204, "Removido")]
    [SwaggerResponse(404, "Não encontrado", typeof(ProblemDetails))]
    public async Task<IActionResult> Remover(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var response = await _fornecedorService.DeleteAsync(id, userId);
        return response is { IsSuccess: true, Data: true } ? DeletedResponse() : HandleApiResponse(response);
    }
}
