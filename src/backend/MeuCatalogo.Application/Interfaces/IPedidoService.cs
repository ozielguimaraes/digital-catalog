using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;

namespace MeuCatalogo.Application.Interfaces;

public interface IPedidoService
{
    Task<ApiResponse<PedidoResponse>> CreateAsync(PedidoRequest request);
    Task<ApiResponse<PedidoResponse>> GetByIdAsync(Guid id);
    Task<ApiResponse<List<PedidoResponse>>> GetAllAsync();
    Task<ApiResponse<List<PedidoResponse>>> GetByClienteAsync(Guid clienteId);
    Task<ApiResponse<bool>> DeleteAsync(Guid id);
}