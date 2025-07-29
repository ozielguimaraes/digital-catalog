using MeuCatalogo.Application.DTOs;
using MeuCatalogo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MeuCatalogo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProdutosController : BaseApiController
{
    private readonly IProdutoService _produtoService;
    private readonly ILogger<ProdutosController> _logger;

    public ProdutosController(IProdutoService produtoService, ILogger<ProdutosController> logger)
    {
        _produtoService = produtoService;
        _logger = logger;
    }

    [HttpGet("catalogo/{catalogoId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProdutoDto>))]
    public async Task<IActionResult> ObterPorCatalogo(Guid catalogoId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Obtendo produtos do catálogo {CatalogoId} para o usuário {UserId}", catalogoId, userId);
        var response = await _produtoService.ObterPorCatalogoIdAsync(catalogoId, userId);
        return HandleApiResponse(response);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProdutoDto))]
    public async Task<IActionResult> Obter(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Obtendo produto {ProdutoId} para o usuário {UserId}", id, userId);
        var response = await _produtoService.ObterPorIdAsync(id, userId);

        return HandleApiResponse(response);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ProdutoDto))]
    public async Task<IActionResult> Adicionar([FromBody] ProdutoCreateDto produtoDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState inválido ao adicionar produto: {@ModelState}", ModelState);
            return ValidationProblemResponse(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Adicionando novo produto para o usuário {UserId}", userId);
        var response = await _produtoService.AdicionarAsync(produtoDto, userId);
        return HandleApiResponse(response);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProdutoDto))]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] ProdutoUpdateDto produtoDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState inválido ao atualizar produto: {@ModelState}", ModelState);
            return ValidationProblemResponse(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Atualizando produto {ProdutoId} para o usuário {UserId}", id, userId);
        var produto = await _produtoService.AtualizarAsync(id, produtoDto, userId);

        return HandleApiResponse(produto);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Remover(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Removendo produto {ProdutoId} para o usuário {UserId}", id, userId);
        var response = await _produtoService.RemoverAsync(id, userId);
        return HandleApiResponse(response);
    }

    [HttpPut("{id}/estoque")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(EstoqueDto))]
    public async Task<IActionResult> AtualizarEstoque(Guid id, [FromBody] EstoqueUpdateDto estoqueDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState inválido ao atualizar estoque: {@ModelState}", ModelState);
            return ValidationProblemResponse(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Atualizando estoque do produto {ProdutoId} para o usuário {UserId}", id, userId);
        var response = await _produtoService.AtualizarEstoqueAsync(id, estoqueDto, userId);

        return HandleApiResponse(response);
    }
}
