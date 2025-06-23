using MeuCatalogo.Application.DTOs;
using MeuCatalogo.Application.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MeuCatalogo.Application.Interfaces;

public interface IPlanoAssinaturaService
{
    Task<IEnumerable<PlanoAssinaturaDto>> ObterTodosAsync();
    Task<PlanoAssinaturaDto> ObterPorIdAsync(Guid id);
    Task<PlanoAssinaturaDto> CriarAsync(PlanoAssinaturaCreateDto planoDto);
    Task<PlanoAssinaturaDto> AtualizarAsync(Guid id, PlanoAssinaturaUpdateDto planoDto);
    Task<bool> ExcluirAsync(Guid id);
    Task<AssinaturaUsuarioDto> AtribuirPlanoGratuitoAsync(string userId);
    Task<AssinaturaUsuarioDto> AssinarPlanoAsync(string userId, Guid planoId, bool renovacaoAutomatica, string? transacaoId = null, string? metodoPagamento = null, decimal valorPago = 0);
    Task<bool> CancelarAssinaturaAsync(string userId, string motivo);
    Task<AssinaturaUsuarioDto?> ObterAssinaturaAtivaAsync(string userId);
    Task<PlanoAssinaturaDto?> ObterPlanoAtivoAsync(string userId);
}