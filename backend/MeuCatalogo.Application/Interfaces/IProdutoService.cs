using MeuCatalogo.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MeuCatalogo.Application.Interfaces;

public interface IProdutoService
{
    Task<IEnumerable<ProdutoDto>> GetProdutosByCatalogoIdAsync(Guid catalogoId, string userId);
    Task<ProdutoDto> GetProdutoByIdAsync(Guid id, string userId);
    Task<ProdutoDto> CreateProdutoAsync(ProdutoCreateDto produtoDto, string userId);
    Task<ProdutoDto> UpdateProdutoAsync(Guid id, ProdutoUpdateDto produtoDto, string userId);
    Task DeleteProdutoAsync(Guid id, string userId);
    Task<EstoqueDto> UpdateEstoqueAsync(Guid produtoId, EstoqueUpdateDto estoqueDto, string userId);
}