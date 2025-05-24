using MeuCatalogo.Application.DTOs;
using MeuCatalogo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MeuCatalogo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CatalogosController : ControllerBase
{
    private readonly ICatalogoService _catalogoService;

    public CatalogosController(ICatalogoService catalogoService)
    {
        _catalogoService = catalogoService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CatalogoDto>>> GetCatalogos()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var catalogos = await _catalogoService.GetCatalogosByUserIdAsync(userId);
        return Ok(catalogos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CatalogoDto>> GetCatalogo(Guid id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var catalogo = await _catalogoService.ObterCatalogoPorIdAsync(id, userId);
            return Ok(catalogo);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<CatalogoDto>> CreateCatalogo(CatalogoCreateDto catalogoDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var catalogo = await _catalogoService.CreateCatalogoAsync(catalogoDto, userId);
        return CreatedAtAction(nameof(GetCatalogo), new { id = catalogo.Id }, catalogo);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CatalogoDto>> UpdateCatalogo(Guid id, CatalogoUpdateDto catalogoDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var catalogo = await _catalogoService.UpdateCatalogoAsync(id, catalogoDto, userId);
            return Ok(catalogo);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCatalogo(Guid id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _catalogoService.DeleteCatalogoAsync(id, userId);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}