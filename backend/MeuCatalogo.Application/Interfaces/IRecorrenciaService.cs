using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;

namespace MeuCatalogo.Application.Interfaces;

public interface IRecorrenciaService
{
    Task<ApiResponse<List<RecorrenciaResponse>>> GetAllAsync(string userId);
    Task<ApiResponse<RecorrenciaResponse>> GetByIdAsync(Guid id, string userId);
    Task<ApiResponse<RecorrenciaResponse>> CreateAsync(RecorrenciaRequest request, string userId);
    Task<ApiResponse<RecorrenciaResponse>> UpdateAsync(Guid id, RecorrenciaRequest request, string userId);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, string userId);
    Task<ApiResponse<bool>> SetAtivoAsync(Guid id, string userId, bool ativo);
    Task<int> GerarPendentesAsync(string userId, DateTime ateData);
}
