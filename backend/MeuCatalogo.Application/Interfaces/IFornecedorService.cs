using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;

namespace MeuCatalogo.Application.Interfaces;

public interface IFornecedorService
{
    Task<ApiResponse<List<FornecedorResponse>>> GetAllAsync(string userId);
    Task<ApiResponse<FornecedorResponse>> GetByIdAsync(Guid id, string userId);
    Task<ApiResponse<FornecedorResponse>> CreateAsync(FornecedorRequest request, string userId);
    Task<ApiResponse<FornecedorResponse>> UpdateAsync(Guid id, FornecedorRequest request, string userId);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, string userId);
}
