using System.Security.Claims;
using MeuCatalogo.Application.DTOs;
using MeuCatalogo.Application.DTOs.Categoria;
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
[SwaggerTag("Gerenciamento de categorias de produtos")]
public class CategoriasController : BaseApiController
{
    private readonly ICategoriaService _categoriaService;
    private readonly ILogger<CategoriasController> _logger;

    public CategoriasController(ILogger<CategoriasController> logger, ICategoriaService categoriaService)
    {
        _logger = logger;
        _categoriaService = categoriaService;
    }

    /// <summary>
    /// Obtém todas as categorias de um catálogo específico
    /// </summary>
    /// <param name="catalogoId">ID do catálogo</param>
    /// <returns>Lista de categorias do catálogo</returns>
    /// <response code="200">Lista de categorias retornada com sucesso</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não tem acesso ao catálogo</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("catalogo/{catalogoId}")]
    [SwaggerOperation(
        Summary = "Listar categorias por catálogo",
        Description = "Retorna todas as categorias de um catálogo específico do usuário autenticado."
    )]
    [SwaggerResponse(200, "Lista de categorias retornada com sucesso", typeof(IEnumerable<CategoriaDto>))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(403, "Usuário não tem acesso ao catálogo", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> ObterPorCatalogo(Guid catalogoId)
    {
        string? usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Obtendo as categorias para o usuário {UserId}, catálogo {CatalogoId}", usuarioId, catalogoId);
        var response = await _categoriaService.ObterPorCatalogoAsync(catalogoId, usuarioId);
        return HandleApiResponse(response);
    }

    /// <summary>
    /// Obtém uma categoria específica por ID
    /// </summary>
    /// <param name="id">ID da categoria</param>
    /// <returns>Dados da categoria</returns>
    /// <response code="200">Categoria encontrada e retornada com sucesso</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não tem acesso à categoria</response>
    /// <response code="404">Categoria não encontrada</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Obter categoria por ID",
        Description = "Retorna os dados de uma categoria específica do usuário autenticado."
    )]
    [SwaggerResponse(200, "Categoria encontrada e retornada com sucesso", typeof(CategoriaResponse))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(403, "Usuário não tem acesso à categoria", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Categoria não encontrada", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Obter(Guid id)
    {
        string? usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Obtendo categoria {CategoriaId} para o usuário {UserId}", id, usuarioId);
        var response = await _categoriaService.ObterPorIdAsync(usuarioId, id);

        return HandleApiResponse(response);
    }

    /// <summary>
    /// Cria uma nova categoria
    /// </summary>
    /// <param name="request">Dados da categoria a ser criada</param>
    /// <returns>Dados da categoria criada</returns>
    /// <response code="201">Categoria criada com sucesso</response>
    /// <response code="400">Dados inválidos fornecidos</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não tem acesso ao catálogo</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Criar nova categoria",
        Description = "Cria uma nova categoria no catálogo do usuário autenticado."
    )]
    [SwaggerResponse(201, "Categoria criada com sucesso", typeof(CategoriaResponse))]
    [SwaggerResponse(400, "Dados inválidos fornecidos", typeof(ProblemDetails))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(403, "Usuário não tem acesso ao catálogo", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Adicionar([FromBody] CategoriaRequest request)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState inválido ao adicionar categoria: {@ModelState}", ModelState);
            return ValidationProblemResponse(ModelState);
        }

        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Adicionando novo categoria para o usuário {UserId}", usuarioId);
        var response = await _categoriaService.AdicionarAsync(usuarioId, request);

        return HandleApiResponse(response);
    }

    /// <summary>
    /// Atualiza uma categoria existente
    /// </summary>
    /// <param name="id">ID da categoria</param>
    /// <param name="categoriaDto">Dados atualizados da categoria</param>
    /// <returns>Dados da categoria atualizada</returns>
    /// <response code="200">Categoria atualizada com sucesso</response>
    /// <response code="400">Dados inválidos fornecidos</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não tem acesso à categoria</response>
    /// <response code="404">Categoria não encontrada</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "Atualizar categoria",
        Description = "Atualiza os dados de uma categoria existente do usuário autenticado."
    )]
    [SwaggerResponse(200, "Categoria atualizada com sucesso", typeof(CategoriaResponse))]
    [SwaggerResponse(400, "Dados inválidos fornecidos", typeof(ProblemDetails))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(403, "Usuário não tem acesso à categoria", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Categoria não encontrada", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarCategoriaRequest categoriaDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState inválido ao atualizar categoria: {@ModelState}", ModelState);
            return ValidationProblemResponse(ModelState);
        }

        string? usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Atualizando categoria {Id} para o usuário {UserId}", id, usuarioId);
        var response = await _categoriaService.AtualizarAsync(id, usuarioId, categoriaDto);

        return HandleApiResponse(response);
    }

    /// <summary>
    /// Remove uma categoria
    /// </summary>
    /// <param name="id">ID da categoria</param>
    /// <returns>Confirmação da remoção</returns>
    /// <response code="204">Categoria removida com sucesso</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não tem acesso à categoria</response>
    /// <response code="404">Categoria não encontrada</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpDelete("{id}")]
    [SwaggerOperation(
        Summary = "Remover categoria",
        Description = "Remove uma categoria do catálogo do usuário autenticado."
    )]
    [SwaggerResponse(204, "Categoria removida com sucesso")]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(403, "Usuário não tem acesso à categoria", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Categoria não encontrada", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Remover(Guid id)
    {
        string? usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Removendo categoria {Id} para o usuário {UserId}", id, usuarioId);
        var response = await _categoriaService.RemoverAsync(id, usuarioId);

        return HandleApiResponse(response);
    }
}
