using MeuCatalogo.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MeuCatalogo.Application.DTOs.Responses;
using Microsoft.AspNetCore.Http;

namespace MeuCatalogo.Application.Interfaces;

public interface IProdutoService
{
    Task<ApiResponse<IEnumerable<ProdutoDto>>> ObterPorCatalogoIdAsync(Guid catalogoId, string userId);
    Task<ApiResponse<IEnumerable<ProdutoDto>>> ObterPorCatalogoIdPublicoAsync(Guid catalogoId);
    Task<ApiResponse<ProdutoDto?>> ObterPorIdAsync(Guid id, string userId);
    Task<ApiResponse<bool>> ExisteAsync(Guid id, string userId);
    Task<ApiResponse<ProdutoDto>> AdicionarAsync(ProdutoCreateDto produtoDto, string userId);
    Task<ApiResponse<ProdutoDto>> AtualizarAsync(Guid id, ProdutoUpdateDto produtoDto, string userId);
    Task<ApiResponse<bool>> RemoverAsync(Guid id, string userId);
    Task<ApiResponse<EstoqueDto>> AtualizarEstoqueAsync(Guid produtoId, EstoqueUpdateDto estoqueDto, string userId);
    Task<ApiResponse<ImageDto>> UploadImagemAsync(Guid produtoId, IFormFile file, string userId, bool isPrincipal = false, int ordem = 0);
}
