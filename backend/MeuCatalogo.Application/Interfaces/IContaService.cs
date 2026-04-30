using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;

namespace MeuCatalogo.Application.Interfaces;

public interface IContaService
{
    Task<ApiResponse<List<ContaResponse>>> GetAllAsync(string userId, bool incluirInativas = false);
    Task<ApiResponse<ContaResponse>> GetByIdAsync(Guid id, string userId);
    Task<ApiResponse<ContaResponse>> CreateAsync(ContaRequest request, string userId);
    Task<ApiResponse<ContaResponse>> UpdateAsync(Guid id, ContaRequest request, string userId);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, string userId);
    Task<ApiResponse<bool>> SetAtivoAsync(Guid id, string userId, bool ativo);
}
