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
    public async Task<ActionResult<IEnumerable<CatalogoDto>>> Obter()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Obtendo catálogos para o usuário {UserId}", userId);
        var catalogos = await _catalogoService.GetCatalogosByUserIdAsync(userId);
        return OkResponse(catalogos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CatalogoDto>> Obter(Guid id)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Obtendo catálogo {CatalogoId} para o usuário {UserId}", id, userId);
        var catalogo = await _catalogoService.ObterCatalogoPorIdAsync(id, userId);

        if (catalogo != null)
        {
            return OkResponse(catalogo);
        }

        _logger.LogWarning("Catálogo {CatalogoId} não encontrado para o usuário {UserId}", id, userId);
        return NotFoundResponse("Catálogo não encontrado");
    }

    [HttpPost]
    public async Task<ActionResult<CatalogoDto>> Adicionar([FromBody] CatalogoCreateDto catalogoDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState inválido ao adicionar catálogo: {@ModelState}", ModelState);
            return ValidationProblemResponse(ModelState);
        }

        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Adicionando novo catálogo para o usuário {UserId}", userId);
        var catalogo = await _catalogoService.CreateCatalogoAsync(catalogoDto, userId);
        return CreatedResponse($"api/catalogos/{catalogo.Id}", catalogo);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CatalogoDto>> Atualizar(Guid id, [FromBody] CatalogoUpdateDto catalogoDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState inválido ao atualizar catálogo: {@ModelState}", ModelState);
            return ValidationProblemResponse(ModelState);
        }

        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Atualizando catálogo {CatalogoId} para o usuário {UserId}", id, userId);
        var catalogo = await _catalogoService.UpdateCatalogoAsync(id, catalogoDto, userId);

        if (catalogo != null)
        {
            return UpdatedResponse(catalogo);
        }

        _logger.LogWarning("Catálogo {CatalogoId} não encontrado para atualização pelo usuário {UserId}", id, userId);
        return NotFoundResponse("Catálogo não encontrado para atualização");
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Remover(Guid id)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Removendo catálogo {CatalogoId} para o usuário {UserId}", id, userId);
        await _catalogoService.DeleteCatalogoAsync(id, userId);
        return DeletedResponse();
    }
}
