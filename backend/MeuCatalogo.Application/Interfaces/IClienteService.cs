using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;

namespace MeuCatalogo.Application.Interfaces;

public interface IClienteService
{
    Task<ApiResponse<ClienteResponse>> CreateAsync(ClienteRequest request);
    Task<ApiResponse<ClienteResponse>> GetByIdAsync(Guid id);
    Task<ApiResponse<ClienteResponse>> GetByEmailAsync(string email);
    Task<ApiResponse<List<ClienteResponse>>> GetAllAsync();
    Task<ApiResponse<ClienteResponse>> UpdateAsync(Guid id, ClienteRequest request);
    Task<ApiResponse<bool>> DeleteAsync(Guid id);
}