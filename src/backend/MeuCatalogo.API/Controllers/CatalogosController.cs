using MeuCatalogo.Application.DTOs;
using MeuCatalogo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Swashbuckle.AspNetCore.Annotations;

namespace MeuCatalogo.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[SwaggerTag("Gerenciamento de catálogos de produtos")]
public class CatalogosController : BaseApiController
{
    private readonly ICatalogoService _catalogoService;
    private readonly ILogger<CatalogosController> _logger;

    public CatalogosController(ICatalogoService catalogoService, ILogger<CatalogosController> logger)
    {
        _catalogoService = catalogoService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém todos os catálogos públicos (sem autenticação)
    /// </summary>
    /// <returns>Lista de catálogos públicos</returns>
    /// <response code="200">Lista de catálogos retornada com sucesso</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Listar catálogos públicos",
        Description = "Retorna todos os catálogos públicos disponíveis."
    )]
    [SwaggerResponse(200, "Lista de catálogos retornada com sucesso", typeof(IEnumerable<CatalogoDto>))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Obter()
    {
        _logger.LogInformation("Obtendo catálogos públicos");
        var response = await _catalogoService.ObterTodosPublicosAsync();

        return HandleApiResponse(response);
    }

    /// <summary>
    /// Obtém todos os catálogos do usuário autenticado
    /// </summary>
    /// <returns>Lista de catálogos do usuário</returns>
    /// <response code="200">Lista de catálogos retornada com sucesso</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("meus")]
    [SwaggerOperation(
        Summary = "Listar catálogos do usuário",
        Description = "Retorna todos os catálogos pertencentes ao usuário autenticado."
    )]
    [SwaggerResponse(200, "Lista de catálogos retornada com sucesso", typeof(IEnumerable<CatalogoDto>))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> ObterMeus()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Obtendo catálogos para o usuário {UserId}", userId);
        var response = await _catalogoService.ObterPorUsuarioIdAsync(userId);

        return HandleApiResponse(response);
    }

    /// <summary>
    /// Obtém um catálogo específico por ID
    /// </summary>
    /// <param name="id">ID do catálogo</param>
    /// <returns>Dados do catálogo</returns>
    /// <response code="200">Catálogo encontrado e retornado com sucesso</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não tem acesso ao catálogo</response>
    /// <response code="404">Catálogo não encontrado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Obter catálogo por ID",
        Description = "Retorna os dados de um catálogo específico do usuário autenticado."
    )]
    [SwaggerResponse(200, "Catálogo encontrado e retornado com sucesso", typeof(CatalogoDto))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(403, "Usuário não tem acesso ao catálogo", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Catálogo não encontrado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Obter(Guid id)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Obtendo catálogo {CatalogoId} para o usuário {UserId}", id, userId);
        var response = await _catalogoService.ObterPorIdAsync(id, userId);

        if (response is { IsSuccess: true, Data: null })
        {
            _logger.LogWarning("Catálogo {CatalogoId} não encontrado para o usuário {UserId}", id, userId);
        }

        return HandleApiResponse(response);
    }

    /// <summary>
    /// Cria um novo catálogo
    /// </summary>
    /// <param name="catalogoDto">Dados do catálogo a ser criado</param>
    /// <returns>Dados do catálogo criado</returns>
    /// <response code="201">Catálogo criado com sucesso</response>
    /// <response code="400">Dados inválidos fornecidos</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Criar novo catálogo",
        Description = "Cria um novo catálogo para o usuário autenticado."
    )]
    [SwaggerResponse(201, "Catálogo criado com sucesso", typeof(CatalogoDto))]
    [SwaggerResponse(400, "Dados inválidos fornecidos", typeof(ProblemDetails))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Adicionar([FromBody] CatalogoCreateDto catalogoDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState inválido ao adicionar catálogo: {@ModelState}", ModelState);
            return ValidationProblemResponse(ModelState);
        }

        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Adicionando novo catálogo para o usuário {UserId}", userId);
        var response = await _catalogoService.AdicionarAsync(catalogoDto, userId);

        return HandleApiResponse(response);
    }

    /// <summary>
    /// Atualiza um catálogo existente
    /// </summary>
    /// <param name="id">ID do catálogo</param>
    /// <param name="catalogoDto">Dados atualizados do catálogo</param>
    /// <returns>Dados do catálogo atualizado</returns>
    /// <response code="200">Catálogo atualizado com sucesso</response>
    /// <response code="400">Dados inválidos fornecidos</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não tem acesso ao catálogo</response>
    /// <response code="404">Catálogo não encontrado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "Atualizar catálogo",
        Description = "Atualiza os dados de um catálogo existente do usuário autenticado."
    )]
    [SwaggerResponse(200, "Catálogo atualizado com sucesso", typeof(CatalogoDto))]
    [SwaggerResponse(400, "Dados inválidos fornecidos", typeof(ProblemDetails))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(403, "Usuário não tem acesso ao catálogo", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Catálogo não encontrado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] CatalogoUpdateDto catalogoDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState inválido ao atualizar catálogo: {@ModelState}", ModelState);
            return ValidationProblemResponse(ModelState);
        }

        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Atualizando catálogo {CatalogoId} para o usuário {UserId}", id, userId);
        var response = await _catalogoService.AtualizarAsync(id, catalogoDto, userId);

        return HandleApiResponse(response);
    }

    /// <summary>
    /// Remove um catálogo
    /// </summary>
    /// <param name="id">ID do catálogo</param>
    /// <returns>Confirmação da remoção</returns>
    /// <response code="204">Catálogo removido com sucesso</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não tem acesso ao catálogo</response>
    /// <response code="404">Catálogo não encontrado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpDelete("{id}")]
    [SwaggerOperation(
        Summary = "Remover catálogo",
        Description = "Remove um catálogo do usuário autenticado."
    )]
    [SwaggerResponse(204, "Catálogo removido com sucesso")]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(403, "Usuário não tem acesso ao catálogo", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Catálogo não encontrado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Remover(Guid id)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        _logger.LogInformation("Removendo catálogo {CatalogoId} para o usuário {UserId}", id, userId);

        var response = await _catalogoService.DeleteCatalogoAsync(id, userId);

        return response is { IsSuccess: true, Data: true } ? DeletedResponse() : HandleApiResponse(response);
    }
}
