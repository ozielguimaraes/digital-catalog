using System.Security.Claims;
using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MeuCatalogo.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/categorias-financeiras")]
[Authorize]
[SwaggerTag("Categorias e subcategorias financeiras (receita/despesa) com ícone e cor.")]
public class CategoriasFinanceirasController : BaseApiController
{
    private readonly ICategoriaFinanceiraService _service;

    public CategoriasFinanceirasController(ICategoriaFinanceiraService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lista categorias financeiras, opcionalmente filtradas pelo tipo (Receita/Despesa).
    /// </summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Listar categorias financeiras",
        Description = "Retorna as categorias do usuário; cada categoria já vem com suas subcategorias. Filtre pelo tipo via query string."
    )]
    [SwaggerResponse(200, "Lista retornada com sucesso", typeof(IEnumerable<CategoriaFinanceiraResponse>))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Listar([FromQuery] CategoriaFinanceiraTipo? tipo)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.GetAllAsync(userId, tipo));
    }

    /// <summary>
    /// Obtém uma categoria pelo ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Obter categoria por ID")]
    [SwaggerResponse(200, "Categoria encontrada", typeof(CategoriaFinanceiraResponse))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Categoria não encontrada", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Obter(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.GetByIdAsync(id, userId));
    }

    /// <summary>
    /// Cria uma categoria financeira.
    /// </summary>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Criar categoria",
        Description = "IconeNome aceita uma chave do catálogo de ícones (ex.: 'shopping-cart'). Cor é hex (#RRGGBB)."
    )]
    [SwaggerResponse(201, "Categoria criada", typeof(CategoriaFinanceiraResponse))]
    [SwaggerResponse(400, "Dados inválidos", typeof(ProblemDetails))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Adicionar([FromBody] CategoriaFinanceiraRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblemResponse(ModelState);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.CreateAsync(request, userId));
    }

    /// <summary>
    /// Atualiza uma categoria.
    /// </summary>
    [HttpPut("{id:guid}")]
    [SwaggerOperation(Summary = "Atualizar categoria")]
    [SwaggerResponse(200, "Categoria atualizada", typeof(CategoriaFinanceiraResponse))]
    [SwaggerResponse(400, "Dados inválidos", typeof(ProblemDetails))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Categoria não encontrada", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] CategoriaFinanceiraRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblemResponse(ModelState);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.UpdateAsync(id, request, userId));
    }

    /// <summary>
    /// Remove uma categoria.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(
        Summary = "Remover categoria",
        Description = "Falha se houver lançamentos referenciando a categoria."
    )]
    [SwaggerResponse(204, "Categoria removida")]
    [SwaggerResponse(400, "Categoria em uso", typeof(ProblemDetails))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Categoria não encontrada", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Remover(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var resp = await _service.DeleteAsync(id, userId);
        return resp is { IsSuccess: true } ? DeletedResponse() : HandleApiResponse(resp);
    }

    /// <summary>
    /// Cria uma subcategoria dentro da categoria informada.
    /// </summary>
    [HttpPost("{id:guid}/subcategorias")]
    [SwaggerOperation(Summary = "Adicionar subcategoria")]
    [SwaggerResponse(201, "Subcategoria criada", typeof(CategoriaFinanceiraResponse))]
    [SwaggerResponse(400, "Dados inválidos", typeof(ProblemDetails))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Categoria não encontrada", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> AdicionarSub(Guid id, [FromBody] SubcategoriaFinanceiraRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblemResponse(ModelState);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.CreateSubcategoriaAsync(id, request, userId));
    }

    /// <summary>
    /// Atualiza uma subcategoria.
    /// </summary>
    [HttpPut("{id:guid}/subcategorias/{subId:guid}")]
    [SwaggerOperation(Summary = "Atualizar subcategoria")]
    [SwaggerResponse(200, "Subcategoria atualizada", typeof(CategoriaFinanceiraResponse))]
    [SwaggerResponse(400, "Dados inválidos", typeof(ProblemDetails))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Subcategoria não encontrada", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> AtualizarSub(Guid id, Guid subId, [FromBody] SubcategoriaFinanceiraRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblemResponse(ModelState);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.UpdateSubcategoriaAsync(id, subId, request, userId));
    }

    /// <summary>
    /// Remove uma subcategoria.
    /// </summary>
    [HttpDelete("{id:guid}/subcategorias/{subId:guid}")]
    [SwaggerOperation(Summary = "Remover subcategoria")]
    [SwaggerResponse(204, "Subcategoria removida")]
    [SwaggerResponse(400, "Subcategoria em uso", typeof(ProblemDetails))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Subcategoria não encontrada", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> RemoverSub(Guid id, Guid subId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var resp = await _service.DeleteSubcategoriaAsync(id, subId, userId);
        return resp is { IsSuccess: true } ? DeletedResponse() : HandleApiResponse(resp);
    }
}
