using System.Security.Claims;
using MeuCatalogo.Application.DTOs;
using MeuCatalogo.Application.DTOs.Categoria;
using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeuCatalogo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriasController : BaseApiController
{
    private readonly ICategoriaService _categoriaService;
    private readonly ILogger<CategoriasController> _logger;

    public CategoriasController(ILogger<CategoriasController> logger, ICategoriaService categoriaService)
    {
        _logger = logger;
        _categoriaService = categoriaService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CategoriaDto>))]
    public async Task<IActionResult> ObterTodas(Guid catalogoId)
    {
        string? usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Obtendo as categorias para o usuário {UserId}, catálogo {CatalogoId}", usuarioId, catalogoId);
        var response = await _categoriaService.ObterPorCatalogoAsync(catalogoId, usuarioId);
        return HandleApiResponse(response);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CategoriaResponse))]
    public async Task<IActionResult> Obter(Guid id)
    {
        string? usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Obtendo categoria {CategoriaId} para o usuário {UserId}", id, usuarioId);
        var response = await _categoriaService.ObterPorIdAsync(usuarioId, id);

        return HandleApiResponse(response);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CategoriaResponse))]
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

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CategoriaResponse))]
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

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Remover(Guid id)
    {
        string? usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Removendo categoria {Id} para o usuário {UserId}", id, usuarioId);
        var response = await _categoriaService.RemoverAsync(id, usuarioId);

        return HandleApiResponse(response);
    }
}
