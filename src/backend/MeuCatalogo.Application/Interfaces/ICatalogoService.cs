using MeuCatalogo.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MeuCatalogo.Application.DTOs.Responses;

namespace MeuCatalogo.Application.Interfaces;

public interface ICatalogoService
{
    Task<ApiResponse<IEnumerable<CatalogoDto>>> ObterTodosPublicosAsync();
    Task<ApiResponse<IEnumerable<CatalogoDto>>> ObterPorUsuarioIdAsync(string userId);
    Task<ApiResponse<CatalogoDto?>> ObterPorIdAsync(Guid id, string userId);
    Task<ApiResponse<Guid?>> ObterCatalogoIdAsync(Guid produtoId, string userId);
    Task<ApiResponse<CatalogoDto>> AdicionarAsync(CatalogoCreateDto catalogoDto, string userId);
    Task<ApiResponse<CatalogoDto>> AtualizarAsync(Guid id, CatalogoUpdateDto catalogoDto, string userId);
    Task<ApiResponse<bool>> DeleteCatalogoAsync(Guid id, string userId);
}
