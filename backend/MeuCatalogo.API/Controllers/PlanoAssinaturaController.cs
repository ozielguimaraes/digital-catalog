using MeuCatalogo.Application.DTOs;
using MeuCatalogo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;

namespace MeuCatalogo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[SwaggerTag("Gerenciamento de planos de assinatura e assinaturas de usuários")]
public class PlanoAssinaturaController : BaseApiController
{
    private readonly IPlanoAssinaturaService _planoAssinaturaService;
    private readonly ILogger<PlanoAssinaturaController> _logger;

    public PlanoAssinaturaController(IPlanoAssinaturaService planoAssinaturaService, ILogger<PlanoAssinaturaController> logger)
    {
        _planoAssinaturaService = planoAssinaturaService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém todos os planos de assinatura disponíveis
    /// </summary>
    /// <returns>Lista de planos de assinatura</returns>
    /// <response code="200">Lista de planos retornada com sucesso</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Listar todos os planos de assinatura",
        Description = "Retorna todos os planos de assinatura disponíveis no sistema. Este endpoint é público e não requer autenticação."
    )]
    [SwaggerResponse(200, "Lista de planos de assinatura retornada com sucesso", typeof(IEnumerable<PlanoAssinaturaDto>))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> ObterTodos()
    {
        _logger.LogInformation("Obtendo todos os planos de assinatura");
        var response = await _planoAssinaturaService.ObterTodosAsync();

        return HandleApiResponse(response);
    }

    /// <summary>
    /// Obtém um plano de assinatura específico por ID
    /// </summary>
    /// <param name="id">ID único do plano de assinatura</param>
    /// <returns>Dados do plano de assinatura</returns>
    /// <response code="200">Plano de assinatura encontrado e retornado com sucesso</response>
    /// <response code="404">Plano de assinatura não encontrado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Obter plano de assinatura por ID",
        Description = "Retorna os dados de um plano de assinatura específico baseado no ID fornecido. Este endpoint é público e não requer autenticação."
    )]
    [SwaggerResponse(200, "Plano de assinatura encontrado e retornado com sucesso", typeof(PlanoAssinaturaDto))]
    [SwaggerResponse(404, "Plano de assinatura não encontrado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        _logger.LogInformation("Obtendo plano de assinatura {PlanoId}", id);

        var response = await _planoAssinaturaService.ObterPorIdAsync(id);

        return HandleApiResponse(response);
    }

    /// <summary>
    /// Cria um novo plano de assinatura
    /// </summary>
    /// <param name="planoDto">Dados do plano de assinatura a ser criado</param>
    /// <returns>Dados do plano de assinatura criado</returns>
    /// <response code="201">Plano de assinatura criado com sucesso</response>
    /// <response code="400">Dados inválidos fornecidos</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não possui permissão de administrador</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(
        Summary = "Criar novo plano de assinatura",
        Description = "Cria um novo plano de assinatura no sistema. Requer permissão de administrador."
    )]
    [SwaggerResponse(201, "Plano de assinatura criado com sucesso", typeof(PlanoAssinaturaDto))]
    [SwaggerResponse(400, "Dados inválidos fornecidos", typeof(ProblemDetails))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(403, "Usuário não possui permissão de administrador", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
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

    /// <summary>
    /// Atualiza um plano de assinatura existente
    /// </summary>
    /// <param name="id">ID único do plano de assinatura</param>
    /// <param name="planoDto">Dados atualizados do plano de assinatura</param>
    /// <returns>Dados do plano de assinatura atualizado</returns>
    /// <response code="200">Plano de assinatura atualizado com sucesso</response>
    /// <response code="400">Dados inválidos fornecidos</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não possui permissão de administrador</response>
    /// <response code="404">Plano de assinatura não encontrado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(
        Summary = "Atualizar plano de assinatura",
        Description = "Atualiza os dados de um plano de assinatura existente. Requer permissão de administrador."
    )]
    [SwaggerResponse(200, "Plano de assinatura atualizado com sucesso", typeof(PlanoAssinaturaDto))]
    [SwaggerResponse(400, "Dados inválidos fornecidos", typeof(ProblemDetails))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(403, "Usuário não possui permissão de administrador", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Plano de assinatura não encontrado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
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

    /// <summary>
    /// Exclui um plano de assinatura
    /// </summary>
    /// <param name="id">ID único do plano de assinatura</param>
    /// <returns>Confirmação da exclusão</returns>
    /// <response code="204">Plano de assinatura excluído com sucesso</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não possui permissão de administrador</response>
    /// <response code="404">Plano de assinatura não encontrado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(
        Summary = "Excluir plano de assinatura",
        Description = "Remove um plano de assinatura do sistema. Requer permissão de administrador."
    )]
    [SwaggerResponse(204, "Plano de assinatura excluído com sucesso")]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(403, "Usuário não possui permissão de administrador", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Plano de assinatura não encontrado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Excluir(Guid id)
    {
        _logger.LogInformation("Excluindo plano de assinatura {PlanoId}", id);

        var response = await _planoAssinaturaService.ExcluirAsync(id);

        return HandleApiResponse(response);
    }

    /// <summary>
    /// Obtém o plano de assinatura ativo do usuário autenticado
    /// </summary>
    /// <returns>Dados do plano de assinatura ativo</returns>
    /// <response code="200">Plano ativo retornado com sucesso</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="404">Usuário não possui plano ativo</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("meu-plano")]
    [SwaggerOperation(
        Summary = "Obter plano ativo do usuário",
        Description = "Retorna o plano de assinatura ativo do usuário autenticado."
    )]
    [SwaggerResponse(200, "Plano ativo retornado com sucesso", typeof(PlanoAssinaturaDto))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Usuário não possui plano ativo", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> ObterMeuPlano()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Obtendo plano ativo do usuário {UserId}", userId);

        var response = await _planoAssinaturaService.ObterPlanoAtivoAsync(userId);
        return HandleApiResponse(response);
    }

    /// <summary>
    /// Obtém a assinatura ativa do usuário autenticado
    /// </summary>
    /// <returns>Dados da assinatura ativa</returns>
    /// <response code="200">Assinatura ativa retornada com sucesso</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="404">Usuário não possui assinatura ativa</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("minha-assinatura")]
    [SwaggerOperation(
        Summary = "Obter assinatura ativa do usuário",
        Description = "Retorna os dados da assinatura ativa do usuário autenticado."
    )]
    [SwaggerResponse(200, "Assinatura ativa retornada com sucesso", typeof(AssinaturaUsuarioDto))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Usuário não possui assinatura ativa", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> ObterMinhaAssinatura()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Obtendo assinatura ativa do usuário {UserId}", userId);

        var response = await _planoAssinaturaService.ObterAssinaturaAtivaAsync(userId);
        return HandleApiResponse(response);
    }

    /// <summary>
    /// Assina um plano de assinatura
    /// </summary>
    /// <param name="planoId">ID do plano de assinatura</param>
    /// <param name="assinaturaDto">Dados da assinatura</param>
    /// <returns>Dados da assinatura criada</returns>
    /// <response code="200">Assinatura realizada com sucesso</response>
    /// <response code="400">Dados inválidos fornecidos</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="404">Plano de assinatura não encontrado</response>
    /// <response code="409">Usuário já possui assinatura ativa</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPost("assinar/{planoId}")]
    [SwaggerOperation(
        Summary = "Assinar plano de assinatura",
        Description = "Permite que o usuário autenticado assine um plano de assinatura específico."
    )]
    [SwaggerResponse(200, "Assinatura realizada com sucesso", typeof(AssinaturaUsuarioDto))]
    [SwaggerResponse(400, "Dados inválidos fornecidos", typeof(ProblemDetails))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Plano de assinatura não encontrado", typeof(ProblemDetails))]
    [SwaggerResponse(409, "Usuário já possui assinatura ativa", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
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

    /// <summary>
    /// Cancela a assinatura ativa do usuário
    /// </summary>
    /// <param name="motivo">Motivo do cancelamento</param>
    /// <returns>Confirmação do cancelamento</returns>
    /// <response code="200">Assinatura cancelada com sucesso</response>
    /// <response code="400">Dados inválidos fornecidos</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="404">Usuário não possui assinatura ativa</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPost("cancelar")]
    [SwaggerOperation(
        Summary = "Cancelar assinatura",
        Description = "Cancela a assinatura ativa do usuário autenticado."
    )]
    [SwaggerResponse(200, "Assinatura cancelada com sucesso")]
    [SwaggerResponse(400, "Dados inválidos fornecidos", typeof(ProblemDetails))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Usuário não possui assinatura ativa", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> CancelarAssinatura([FromBody] string motivo)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Usuário {UserId} solicitando cancelamento de assinatura", userId);

        var resultado = await _planoAssinaturaService.CancelarAssinaturaAsync(userId, motivo);
        return HandleApiResponse(resultado);
    }
}
