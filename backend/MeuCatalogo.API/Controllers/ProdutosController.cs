using MeuCatalogo.Application.DTOs;
using MeuCatalogo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MeuCatalogo.Application.DTOs.Responses;

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
    public async Task<ActionResult<IEnumerable<ProdutoDto>>> ObterPorCatalogo(Guid catalogoId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Obtendo produtos do catálogo {CatalogoId} para o usuário {UserId}", catalogoId, userId);
        var produtos = await _produtoService.GetProdutosByCatalogoIdAsync(catalogoId, userId);
        return OkResponse(produtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProdutoDto>> Obter(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Obtendo produto {ProdutoId} para o usuário {UserId}", id, userId);
        var produto = await _produtoService.GetProdutoByIdAsync(id, userId);

        if (produto == null)
        {
            _logger.LogWarning("Produto {ProdutoId} não encontrado para o usuário {UserId}", id, userId);
            return NotFoundResponse("Produto não encontrado");
        }

        return OkResponse(produto);
    }

    [HttpPost]
    public async Task<ActionResult<ProdutoDto>> Adicionar([FromBody] ProdutoCreateDto produtoDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState inválido ao adicionar produto: {@ModelState}", ModelState);
            return ValidationProblemResponse(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Adicionando novo produto para o usuário {UserId}", userId);
        var produto = await _produtoService.CreateProdutoAsync(produtoDto, userId);
        return CreatedResponse($"api/produtos/{produto.Id}", produto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProdutoDto>> Atualizar(Guid id, [FromBody] ProdutoUpdateDto produtoDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState inválido ao atualizar produto: {@ModelState}", ModelState);
            return ValidationProblemResponse(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Atualizando produto {ProdutoId} para o usuário {UserId}", id, userId);
        var produto = await _produtoService.UpdateProdutoAsync(id, produtoDto, userId);

        if (produto == null)
        {
            _logger.LogWarning("Produto {ProdutoId} não encontrado para atualização pelo usuário {UserId}", id, userId);
            return NotFoundResponse("Produto não encontrado para atualização");
        }

        return UpdatedResponse(produto);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Remover(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Removendo produto {ProdutoId} para o usuário {UserId}", id, userId);
        await _produtoService.DeleteProdutoAsync(id, userId);
        return DeletedResponse();
    }

    [HttpPut("{id}/estoque")]
    public async Task<ActionResult<EstoqueDto>> AtualizarEstoque(Guid id, [FromBody] EstoqueUpdateDto estoqueDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState inválido ao atualizar estoque: {@ModelState}", ModelState);
            return ValidationProblemResponse(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Atualizando estoque do produto {ProdutoId} para o usuário {UserId}", id, userId);
        var estoque = await _produtoService.UpdateEstoqueAsync(id, estoqueDto, userId);

        if (estoque == null)
        {
            _logger.LogWarning("Produto {ProdutoId} não encontrado para atualização de estoque pelo usuário {UserId}", id, userId);
            return NotFoundResponse("Produto não encontrado para atualização de estoque");
        }

        return UpdatedResponse(estoque);
    }
}
