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
    public async Task<IActionResult> ObterTodos()
    {
        _logger.LogInformation("Obtendo todos os planos de assinatura");
        var response = await _planoAssinaturaService.ObterTodosAsync();

        return HandleApiResponse(response);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PlanoAssinaturaDto))]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        _logger.LogInformation("Obtendo plano de assinatura {PlanoId}", id);

        var response = await _planoAssinaturaService.ObterPorIdAsync(id);

        return HandleApiResponse(response);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(PlanoAssinaturaDto))]
    public async Task<IActionResult> Criar([FromBody] PlanoAssinaturaCreateDto planoDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState inválido ao criar plano: {@ModelState}", ModelState);
            return ValidationProblemResponse(ModelState);
        }

        _logger.LogInformation("Criando novo plano de assinatura");
        var response = await _planoAssinaturaService.CriarAsync(planoDto);

        return HandleApiResponse(response);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PlanoAssinaturaDto))]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] PlanoAssinaturaUpdateDto planoDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState inválido ao atualizar plano: {@ModelState}", ModelState);
            return ValidationProblemResponse(ModelState);
        }

        _logger.LogInformation("Atualizando plano de assinatura {PlanoId}", id);

        var response = await _planoAssinaturaService.AtualizarAsync(id, planoDto);

        return HandleApiResponse(response);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Excluir(Guid id)
    {
        _logger.LogInformation("Excluindo plano de assinatura {PlanoId}", id);

        var response = await _planoAssinaturaService.ExcluirAsync(id);

        return HandleApiResponse(response);
    }

    [HttpGet("meu-plano")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PlanoAssinaturaDto))]
    public async Task<IActionResult> ObterMeuPlano()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Obtendo plano ativo do usuário {UserId}", userId);

        var response = await _planoAssinaturaService.ObterPlanoAtivoAsync(userId);
        return HandleApiResponse(response);
    }

    [HttpGet("minha-assinatura")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AssinaturaUsuarioDto))]
    public async Task<IActionResult> ObterMinhaAssinatura()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Obtendo assinatura ativa do usuário {UserId}", userId);

        var response = await _planoAssinaturaService.ObterAssinaturaAtivaAsync(userId);
        return HandleApiResponse(response);
    }

    [HttpPost("assinar/{planoId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AssinaturaUsuarioDto))]
    public async Task<IActionResult> AssinarPlano(Guid planoId,
        [FromBody] AssinaturaUsuarioCreateDto assinaturaDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Usuário {UserId} assinando plano {PlanoId}", userId, planoId);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState inválido ao assinar plano: {@ModelState}", ModelState);
            return ValidationProblemResponse(ModelState);
        }

        var response = await _planoAssinaturaService.AssinarPlanoAsync(
            userId,
            planoId,
            assinaturaDto.RenovacaoAutomatica,
            assinaturaDto.TransacaoId,
            assinaturaDto.MetodoPagamento,
            assinaturaDto.ValorPago);

        return HandleApiResponse(response);
    }

    [HttpPost("cancelar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelarAssinatura([FromBody] string motivo)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Usuário {UserId} solicitando cancelamento de assinatura", userId);

        var resultado = await _planoAssinaturaService.CancelarAssinaturaAsync(userId, motivo);
        return HandleApiResponse(resultado);
    }
}
