using MeuCatalogo.Application.DTOs;
using MeuCatalogo.Application.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MeuCatalogo.Application.DTOs.Responses;

namespace MeuCatalogo.Application.Interfaces;

public interface IPlanoAssinaturaService
{
    Task<ApiResponse<IEnumerable<PlanoAssinaturaDto>>> ObterTodosAsync();
    Task<ApiResponse<PlanoAssinaturaDto>> ObterPorIdAsync(Guid id);
    Task<ApiResponse<PlanoAssinaturaDto>> CriarAsync(PlanoAssinaturaCreateDto planoDto);
    Task<ApiResponse<PlanoAssinaturaDto>> AtualizarAsync(Guid id, PlanoAssinaturaUpdateDto planoDto);
    Task<ApiResponse<bool>> ExcluirAsync(Guid id);
    Task<ApiResponse<AssinaturaUsuarioDto>> AtribuirPlanoGratuitoAsync(string userId);
    Task<ApiResponse<AssinaturaUsuarioDto>> AssinarPlanoAsync(string userId, Guid planoId, bool renovacaoAutomatica, string? transacaoId = null, string? metodoPagamento = null, decimal valorPago = 0);
    Task<ApiResponse<bool>> CancelarAssinaturaAsync(string userId, string motivo);
    Task<ApiResponse<AssinaturaUsuarioDto?>> ObterAssinaturaAtivaAsync(string userId);
    Task<ApiResponse<PlanoAssinaturaDto?>> ObterPlanoAtivoAsync(string userId);
}
