using MeuCatalogo.Application.DTOs;
using MeuCatalogo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MeuCatalogo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CatalogosController : BaseApiController
{
    private readonly ICatalogoService _catalogoService;
    private readonly ILogger<CatalogosController> _logger;

    public CatalogosController(ICatalogoService catalogoService, ILogger<CatalogosController> logger)
    {
        _catalogoService = catalogoService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CatalogoDto>))]
    public async Task<IActionResult> Obter()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Obtendo catálogos para o usuário {UserId}", userId);
        var response = await _catalogoService.ObterPorUsuarioIdAsync(userId);

        return HandleApiResponse(response);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CatalogoDto))]
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

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CatalogoDto))]
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

    [HttpPut("{id}")]
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

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> Remover(Guid id)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        _logger.LogInformation("Removendo catálogo {CatalogoId} para o usuário {UserId}", id, userId);

        var response = await _catalogoService.DeleteCatalogoAsync(id, userId);

        return response is { IsSuccess: true, Data: true } ? DeletedResponse() : HandleApiResponse(response);
    }
}
