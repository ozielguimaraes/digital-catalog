using System.Security.Claims;
using MeuCatalogo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeuCatalogo.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/comprovantes-financeiros")]
[Authorize]
public class ComprovantesFinanceirosController : BaseApiController
{
    private readonly IComprovanteFinanceiroService _service;

    public ComprovantesFinanceirosController(IComprovanteFinanceiroService service)
    {
        _service = service;
    }

    [HttpPost]
    [RequestSizeLimit(15 * 1024 * 1024)]
    public async Task<IActionResult> Upload([FromForm] IFormFile arquivo, [FromForm] string? descricao)
    {
        if (arquivo == null || arquivo.Length == 0)
            return BadRequestResponse("Arquivo obrigatório");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await using var stream = arquivo.OpenReadStream();
        var resp = await _service.UploadAsync(userId, arquivo.FileName, arquivo.ContentType, arquivo.Length, stream, descricao);
        return HandleApiResponse(resp);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Obter(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.GetByIdAsync(id, userId));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var resp = await _service.DeleteAsync(id, userId);
        return resp is { IsSuccess: true } ? DeletedResponse() : HandleApiResponse(resp);
    }
}
