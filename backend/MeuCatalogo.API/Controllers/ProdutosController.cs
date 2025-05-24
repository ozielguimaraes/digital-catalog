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
public class ProdutosController : ControllerBase
{
    private readonly IProdutoService _produtoService;

    public ProdutosController(IProdutoService produtoService)
    {
        _produtoService = produtoService;
    }

    [HttpGet("catalogo/{catalogoId}")]
    public async Task<ActionResult<IEnumerable<ProdutoDto>>> GetProdutosByCatalogo(Guid catalogoId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var produtos = await _produtoService.GetProdutosByCatalogoIdAsync(catalogoId, userId);
            return Ok(produtos);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProdutoDto>> GetProduto(Guid id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var produto = await _produtoService.GetProdutoByIdAsync(id, userId);
            return Ok(produto);
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
    public async Task<ActionResult<ProdutoDto>> CreateProduto(ProdutoCreateDto produtoDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var produto = await _produtoService.CreateProdutoAsync(produtoDto, userId);
            return CreatedAtAction(nameof(GetProduto), new { id = produto.Id }, produto);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProdutoDto>> UpdateProduto(Guid id, ProdutoUpdateDto produtoDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var produto = await _produtoService.UpdateProdutoAsync(id, produtoDto, userId);
            return Ok(produto);
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
    public async Task<ActionResult> DeleteProduto(Guid id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _produtoService.DeleteProdutoAsync(id, userId);
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

    [HttpPut("{id}/estoque")]
    public async Task<ActionResult<EstoqueDto>> UpdateEstoque(Guid id, EstoqueUpdateDto estoqueDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var estoque = await _produtoService.UpdateEstoqueAsync(id, estoqueDto, userId);
            return Ok(estoque);
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