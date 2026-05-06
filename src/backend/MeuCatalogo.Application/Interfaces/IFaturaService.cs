using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;

namespace MeuCatalogo.Application.Interfaces;

public interface IFaturaService
{
    Task<ApiResponse<FaturaResponse>> GetAsync(Guid contaId, int mes, int ano, string userId);
    Task<ApiResponse<List<FaturaResponse>>> ListarPorContaAsync(Guid contaId, string userId, int? ano = null);
    Task<Fatura> ObterOuCriarAsync(Conta cartao, int mes, int ano);
    Task<ApiResponse<bool>> RegistrarPagamentoAsync(Guid faturaId, decimal valor, string userId);
}
