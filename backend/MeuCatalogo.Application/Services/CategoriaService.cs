using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Interfaces;
using MeuCatalogo.Application.Infrastructure.Data;
using MeuCatalogo.Application.Infrastructure.Data.Repository;

namespace MeuCatalogo.Application.Services;

public class CategoriaService : ICategoriaService
{
    private readonly ApplicationDbContext _dbContext;

    public CategoriaService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApiResponse<CategoriaResponse>> CreateAsync(CategoriaRequest request)
    {
        try
        {
            var categoria = new Categoria
            {
                Nome = request.Nome
            };

            await _dbContext.AddCategoriaAsync(categoria);

            return ApiResponse<CategoriaResponse>.SuccessResponse(MapToResponse(categoria));
        }
        catch (Exception ex)
        {
            return ApiResponse<CategoriaResponse>.ErrorResponse($"Erro ao criar categoria: {ex.Message}");
        }
    }

    public async Task<ApiResponse<CategoriaResponse>> GetByIdAsync(Guid id)
    {
        try
        {
            var categoria = await _dbContext.GetCategoriaByIdAsync(id);

            if (categoria == null)
                return ApiResponse<CategoriaResponse>.ErrorResponse("Categoria não encontrada");

            return ApiResponse<CategoriaResponse>.SuccessResponse(MapToResponse(categoria));
        }
        catch (Exception ex)
        {
            return ApiResponse<CategoriaResponse>.ErrorResponse($"Erro ao buscar categoria: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<CategoriaResponse>>> GetAllAsync()
    {
        try
        {
            var categorias = await _dbContext.GetAllCategoriasAsync();
            var response = categorias.Select(c => MapToResponse(c)).ToList();

            return ApiResponse<List<CategoriaResponse>>.SuccessResponse(response);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<CategoriaResponse>>.ErrorResponse($"Erro ao buscar categorias: {ex.Message}");
        }
    }

    public async Task<ApiResponse<CategoriaResponse>> UpdateAsync(Guid id, CategoriaRequest request)
    {
        try
        {
            var categoria = await _dbContext.Categorias.FindAsync(id);

            if (categoria == null)
                return ApiResponse<CategoriaResponse>.ErrorResponse("Categoria não encontrada");

            categoria.Nome = request.Nome;

            _dbContext.Categorias.Update(categoria);
            await _dbContext.SaveChangesAsync();

            return ApiResponse<CategoriaResponse>.SuccessResponse(MapToResponse(categoria));
        }
        catch (Exception ex)
        {
            return ApiResponse<CategoriaResponse>.ErrorResponse($"Erro ao atualizar categoria: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        try
        {
            var categoria = await _dbContext.Categorias.FindAsync(id);

            if (categoria == null)
                return ApiResponse<bool>.ErrorResponse("Categoria não encontrada");

            // Verificar se existem produtos associados a esta categoria
            var produtosAssociados = await _dbContext.Produtos
                .AnyAsync(p => p.CategoriaId == id);

            if (produtosAssociados)
                return ApiResponse<bool>.ErrorResponse("não u00e9 possu00edvel excluir a categoria pois existem produtos associados a ela");

            _dbContext.Categorias.Remove(categoria);
            await _dbContext.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Categoria removida com sucesso");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResponse($"Erro ao remover categoria: {ex.Message}");
        }
    }

    private CategoriaResponse MapToResponse(Categoria categoria)
    {
        return new CategoriaResponse
        {
            Id = categoria.Id,
            Nome = categoria.Nome,
        };
    }
}
