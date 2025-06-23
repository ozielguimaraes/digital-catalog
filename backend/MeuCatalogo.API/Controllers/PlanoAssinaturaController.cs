using MeuCatalogo.Application.DTOs;
using MeuCatalogo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MeuCatalogo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlanoAssinaturaController : BaseApiController
{
    private readonly IPlanoAssinaturaService _planoAssinaturaService;
    private readonly ILogger<PlanoAssinaturaController> _logger;

    public PlanoAssinaturaController(IPlanoAssinaturaService planoAssinaturaService, ILogger<PlanoAssinaturaController> logger)
    {
        _planoAssinaturaService = planoAssinaturaService;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<PlanoAssinaturaDto>>> ObterTodos()
    {
        _logger.LogInformation("Obtendo todos os planos de assinatura");
        var planos = await _planoAssinaturaService.ObterTodosAsync();
        return OkResponse(planos);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<PlanoAssinaturaDto>> ObterPorId(Guid id)
    {
        _logger.LogInformation("Obtendo plano de assinatura {PlanoId}", id);

        var plano = await _planoAssinaturaService.ObterPorIdAsync(id);
        if (plano == null)
        {
            _logger.LogWarning("Plano de assinatura {PlanoId} não encontrado", id);
            return NotFoundResponse("Plano de assinatura não encontrado");
        }

        return OkResponse(plano);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PlanoAssinaturaDto>> Criar([FromBody] PlanoAssinaturaCreateDto planoDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState inválido ao criar plano: {@ModelState}", ModelState);
            return ValidationProblemResponse(ModelState);
        }

        _logger.LogInformation("Criando novo plano de assinatura");
        var plano = await _planoAssinaturaService.CriarAsync(planoDto);
        return CreatedResponse(Url.Action(nameof(ObterPorId), new { id = plano.Id }), plano);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PlanoAssinaturaDto>> Atualizar(Guid id, [FromBody] PlanoAssinaturaUpdateDto planoDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState inválido ao atualizar plano: {@ModelState}", ModelState);
            return ValidationProblemResponse(ModelState);
        }

        _logger.LogInformation("Atualizando plano de assinatura {PlanoId}", id);

        var plano = await _planoAssinaturaService.AtualizarAsync(id, planoDto);
        if (plano == null)
        {
            _logger.LogWarning("Plano de assinatura {PlanoId} não encontrado para atualização", id);
            return NotFoundResponse("Plano de assinatura não encontrado");
        }

        return UpdatedResponse(plano);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Excluir(Guid id)
    {
        _logger.LogInformation("Excluindo plano de assinatura {PlanoId}", id);

        var resultado = await _planoAssinaturaService.ExcluirAsync(id);
        if (resultado)
        {
            return DeletedResponse();
        }

        _logger.LogWarning("Plano de assinatura {PlanoId} não encontrado para exclusão", id);
        return NotFoundResponse("Plano de assinatura não encontrado");
    }

    [HttpGet("meu-plano")]
    public async Task<ActionResult<PlanoAssinaturaDto>> ObterMeuPlano()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Obtendo plano ativo do usuário {UserId}", userId);

        var plano = await _planoAssinaturaService.ObterPlanoAtivoAsync(userId);
        if (plano == null)
        {
            return NotFoundResponse("Você não possui um plano ativo.");
        }

        return OkResponse(plano);
    }

    [HttpGet("minha-assinatura")]
    public async Task<ActionResult<AssinaturaUsuarioDto>> ObterMinhaAssinatura()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Obtendo assinatura ativa do usuário {UserId}", userId);

        var assinatura = await _planoAssinaturaService.ObterAssinaturaAtivaAsync(userId);
        if (assinatura == null)
        {
            return NotFoundResponse("Você não possui uma assinatura ativa.");
        }

        return OkResponse(assinatura);
    }

    [HttpPost("assinar/{planoId}")]
    public async Task<ActionResult<AssinaturaUsuarioDto>> AssinarPlano(Guid planoId,
        [FromBody] AssinaturaUsuarioCreateDto assinaturaDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Usuário {UserId} assinando plano {PlanoId}", userId, planoId);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState inválido ao assinar plano: {@ModelState}", ModelState);
            return ValidationProblemResponse(ModelState);
        }

        var assinatura = await _planoAssinaturaService.AssinarPlanoAsync(
            userId,
            planoId,
            assinaturaDto.RenovacaoAutomatica,
            assinaturaDto.TransacaoId,
            assinaturaDto.MetodoPagamento,
            assinaturaDto.ValorPago);

        return OkResponse(assinatura);
    }

    [HttpPost("cancelar")]
    public async Task<ActionResult> CancelarAssinatura([FromBody] string motivo)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Usuário {UserId} solicitando cancelamento de assinatura", userId);

        var resultado = await _planoAssinaturaService.CancelarAssinaturaAsync(userId, motivo);
        if (resultado)
        {
            return NoContent();
        }

        return NotFoundResponse("Você não possui uma assinatura ativa para cancelar.");
    }
}
