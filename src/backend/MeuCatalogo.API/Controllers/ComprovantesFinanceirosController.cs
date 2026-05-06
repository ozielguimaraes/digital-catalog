using System.Security.Claims;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MeuCatalogo.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/comprovantes-financeiros")]
[Authorize]
[SwaggerTag("Upload e download de comprovantes financeiros (multipart/form-data).")]
public class ComprovantesFinanceirosController : BaseApiController
{
    private readonly IComprovanteFinanceiroService _service;

    public ComprovantesFinanceirosController(IComprovanteFinanceiroService service)
    {
        _service = service;
    }

    /// <summary>
    /// Faz upload de um comprovante (PDF, JPG, PNG). Limite de 15 MB por arquivo.
    /// </summary>
    [HttpPost]
    [RequestSizeLimit(15 * 1024 * 1024)]
    [SwaggerOperation(
        Summary = "Upload de comprovante",
        Description = "Multipart com campo 'arquivo' (file) e 'descricao' (string opcional). Tamanho máximo: 15 MB."
    )]
    [SwaggerResponse(201, "Comprovante criado", typeof(ComprovanteFinanceiroResponse))]
    [SwaggerResponse(400, "Arquivo ausente ou tipo inválido", typeof(ProblemDetails))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(413, "Arquivo excede 15 MB", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Upload([FromForm] IFormFile arquivo, [FromForm] string? descricao)
    {
        if (arquivo == null || arquivo.Length == 0)
            return BadRequestResponse("Arquivo obrigatório");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await using var stream = arquivo.OpenReadStream();
        var resp = await _service.UploadAsync(userId, arquivo.FileName, arquivo.ContentType, arquivo.Length, stream, descricao);
        return HandleApiResponse(resp);
    }

    /// <summary>
    /// Obtém metadados de um comprovante (URL absoluta para download).
    /// </summary>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Obter comprovante por ID")]
    [SwaggerResponse(200, "Comprovante encontrado", typeof(ComprovanteFinanceiroResponse))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Comprovante não encontrado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Obter(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return HandleApiResponse(await _service.GetByIdAsync(id, userId));
    }

    /// <summary>
    /// Remove o comprovante e o blob no storage.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Remover comprovante")]
    [SwaggerResponse(204, "Comprovante removido")]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Comprovante não encontrado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Remover(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var resp = await _service.DeleteAsync(id, userId);
        return resp is { IsSuccess: true } ? DeletedResponse() : HandleApiResponse(resp);
    }
}
