using System.Security.Claims;
using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeuCatalogo.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/categorias-financeiras")]
[Authorize]
public class CategoriasFinanceirasController : BaseApiController
{
    private readonly ICategoriaFinanceiraService _service;

    public CategoriasFinanceirasController(ICategoriaFinanceiraService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] CategoriaFinanceiraTipo? tipo)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.GetAllAsync(userId, tipo));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Obter(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.GetByIdAsync(id, userId));
    }

    [HttpPost]
    public async Task<IActionResult> Adicionar([FromBody] CategoriaFinanceiraRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblemResponse(ModelState);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.CreateAsync(request, userId));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] CategoriaFinanceiraRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblemResponse(ModelState);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.UpdateAsync(id, request, userId));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var resp = await _service.DeleteAsync(id, userId);
        return resp is { IsSuccess: true } ? DeletedResponse() : HandleApiResponse(resp);
    }

    [HttpPost("{id:guid}/subcategorias")]
    public async Task<IActionResult> AdicionarSub(Guid id, [FromBody] SubcategoriaFinanceiraRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblemResponse(ModelState);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.CreateSubcategoriaAsync(id, request, userId));
    }

    [HttpPut("{id:guid}/subcategorias/{subId:guid}")]
    public async Task<IActionResult> AtualizarSub(Guid id, Guid subId, [FromBody] SubcategoriaFinanceiraRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblemResponse(ModelState);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.UpdateSubcategoriaAsync(id, subId, request, userId));
    }

    [HttpDelete("{id:guid}/subcategorias/{subId:guid}")]
    public async Task<IActionResult> RemoverSub(Guid id, Guid subId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var resp = await _service.DeleteSubcategoriaAsync(id, subId, userId);
        return resp is { IsSuccess: true } ? DeletedResponse() : HandleApiResponse(resp);
    }
}
