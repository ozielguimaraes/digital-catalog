using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;

namespace MeuCatalogo.Application.Interfaces;

public interface ICategoriaFinanceiraService
{
    Task<ApiResponse<List<CategoriaFinanceiraResponse>>> GetAllAsync(string userId, CategoriaFinanceiraTipo? tipo = null);
    Task<ApiResponse<CategoriaFinanceiraResponse>> GetByIdAsync(Guid id, string userId);
    Task<ApiResponse<CategoriaFinanceiraResponse>> CreateAsync(CategoriaFinanceiraRequest request, string userId);
    Task<ApiResponse<CategoriaFinanceiraResponse>> UpdateAsync(Guid id, CategoriaFinanceiraRequest request, string userId);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, string userId);

    Task<ApiResponse<SubcategoriaFinanceiraResponse>> CreateSubcategoriaAsync(Guid categoriaId, SubcategoriaFinanceiraRequest request, string userId);
    Task<ApiResponse<SubcategoriaFinanceiraResponse>> UpdateSubcategoriaAsync(Guid categoriaId, Guid subcategoriaId, SubcategoriaFinanceiraRequest request, string userId);
    Task<ApiResponse<bool>> DeleteSubcategoriaAsync(Guid categoriaId, Guid subcategoriaId, string userId);
}
