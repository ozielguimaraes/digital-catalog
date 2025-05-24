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
public class PlanoAssinaturaController : ControllerBase
{
    private readonly IPlanoAssinaturaService _planoAssinaturaService;

    public PlanoAssinaturaController(IPlanoAssinaturaService planoAssinaturaService)
    {
        _planoAssinaturaService = planoAssinaturaService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlanoAssinaturaDto>>> ObterTodos()
    {
        var planos = await _planoAssinaturaService.ObterTodosAsync();
        return Ok(planos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PlanoAssinaturaDto>> ObterPorId(Guid id)
    {
        try
        {
            var plano = await _planoAssinaturaService.ObterPorIdAsync(id);
            return Ok(plano);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PlanoAssinaturaDto>> Criar(PlanoAssinaturaCreateDto planoDto)
    {
        var plano = await _planoAssinaturaService.CriarAsync(planoDto);
        return CreatedAtAction(nameof(ObterPorId), new { id = plano.Id }, plano);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PlanoAssinaturaDto>> Atualizar(Guid id, PlanoAssinaturaUpdateDto planoDto)
    {
        try
        {
            var plano = await _planoAssinaturaService.AtualizarAsync(id, planoDto);
            return Ok(plano);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Excluir(Guid id)
    {
        try
        {
            var resultado = await _planoAssinaturaService.ExcluirAsync(id);
            if (resultado)
            {
                return NoContent();
            }
            return NotFound($"Plano de assinatura com ID {id} não encontrado.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("meu-plano")]
    [Authorize]
    public async Task<ActionResult<PlanoAssinaturaDto>> ObterMeuPlano()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var plano = await _planoAssinaturaService.ObterPlanoAtivoAsync(userId);

        if (plano == null)
        {
            return NotFound("Você não possui um plano ativo.");
        }

        return Ok(plano);
    }

    [HttpGet("minha-assinatura")]
    [Authorize]
    public async Task<ActionResult<AssinaturaUsuarioDto>> ObterMinhaAssinatura()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var assinatura = await _planoAssinaturaService.ObterAssinaturaAtivaAsync(userId);

        if (assinatura == null)
        {
            return NotFound("Você não possui uma assinatura ativa.");
        }

        return Ok(assinatura);
    }

    [HttpPost("assinar/{planoId}")]
    [Authorize]
    public async Task<ActionResult<AssinaturaUsuarioDto>> AssinarPlano(Guid planoId, [FromBody] AssinaturaUsuarioCreateDto assinaturaDto)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        try
        {
            var assinatura = await _planoAssinaturaService.AssinarPlanoAsync(
                userId,
                planoId,
                assinaturaDto.RenovacaoAutomatica,
                assinaturaDto.TransacaoId,
                assinaturaDto.MetodoPagamento,
                assinaturaDto.ValorPago);

            return Ok(assinatura);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("cancelar")]
    [Authorize]
    public async Task<ActionResult> CancelarAssinatura([FromBody] string motivo)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var resultado = await _planoAssinaturaService.CancelarAssinaturaAsync(userId, motivo);

        if (resultado)
        {
            return NoContent();
        }

        return NotFound("Você não possui uma assinatura ativa para cancelar.");
    }
}
