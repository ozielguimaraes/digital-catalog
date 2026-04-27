using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;

namespace MeuCatalogo.Application.Interfaces;

public interface IFinanceiroService
{
    Task<ApiResponse<List<LancamentoResponse>>> GetAllAsync(string userId, LancamentoTipo? tipo = null);
    Task<ApiResponse<LancamentoResponse>> GetByIdAsync(Guid id, string userId);
    Task<ApiResponse<LancamentoResponse>> CreateAsync(LancamentoRequest request, string userId);
    Task<ApiResponse<LancamentoResponse>> UpdateAsync(Guid id, LancamentoRequest request, string userId);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, string userId);
    Task<ApiResponse<FinanceiroResumoResponse>> GetResumoAsync(string userId);
}
