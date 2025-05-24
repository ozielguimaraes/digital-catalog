using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;

namespace MeuCatalogo.Application.Interfaces;

public interface ICategoriaService
{
    Task<ApiResponse<CategoriaResponse>> CreateAsync(CategoriaRequest request);
    Task<ApiResponse<CategoriaResponse>> GetByIdAsync(Guid id);
    Task<ApiResponse<List<CategoriaResponse>>> GetAllAsync();
    Task<ApiResponse<CategoriaResponse>> UpdateAsync(Guid id, CategoriaRequest request);
    Task<ApiResponse<bool>> DeleteAsync(Guid id);
}