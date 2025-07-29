using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MeuCatalogo.Application.DTOs.Categoria;
using Microsoft.EntityFrameworkCore;
using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Interfaces;
using MeuCatalogo.Application.Infrastructure.Data;
using MeuCatalogo.Application.Infrastructure.Data.Repository;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Application.Services;

public class CategoriaService : ICategoriaService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<CategoriaService> _logger;

    public CategoriaService(ApplicationDbContext dbContext, ILogger<CategoriaService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ApiResponse<CategoriaResponse>> AdicionarAsync(string usuarioId, CategoriaRequest request)
    {
        try
        {
            var categoria = new Categoria
            {
                Nome = request.Nome,
                CatalogoId = request.CatalogoId,
                Descricao = request.Descricao
            };

            await _dbContext.AddCategoriaAsync(categoria);

            return ApiResponse<CategoriaResponse>.Success(MapToResponse(categoria));
        }
        catch (Exception ex)
        {
            return ApiResponse<CategoriaResponse>.Error($"Erro ao criar categoria: {ex.Message}");
        }
    }

    public async Task<ApiResponse<CategoriaResponse>> ObterPorIdAsync(string usuarioId, Guid id)
    {
        try
        {
            var categoria = await _dbContext.GetCategoriaByIdAsync(id);

            if (categoria == null)
                return ApiResponse<CategoriaResponse>.Error("Categoria não encontrada");

            return ApiResponse<CategoriaResponse>.Success(MapToResponse(categoria));
        }
        catch (Exception ex)
        {
            return ApiResponse<CategoriaResponse>.Error($"Erro ao buscar categoria: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IList<CategoriaResponse>>> ObterPorCatalogoAsync(Guid catalogoId, string usuarioId)
    {
        try
        {
            _logger.LogInformation($"Obter categorias do catálogo: {catalogoId}");
            var categorias = await _dbContext.GetAllCategoriasAsync();
            var response = categorias.Select(MapToResponse).ToList();

            return ApiResponse<IList<CategoriaResponse>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter categorias");
            throw;
        }
    }

    public async Task<ApiResponse<CategoriaResponse>> AtualizarAsync(Guid id, string usuarioId, AtualizarCategoriaRequest request)
    {
        var categoria = await _dbContext.Categorias.FindAsync(id);

        if (categoria == null)
        {
            _logger.LogWarning("Categoria com ID {Id} não encontrada para atualização", id);
            return ApiResponse<CategoriaResponse>.Error(ResponseType.NotFound, "Categoria não encontrada");
        }

        categoria.Nome = request.Nome;
        categoria.Descricao = request.Descricao;
        categoria.DataAtualizacao = DateTime.Now;

        _dbContext.Categorias.Update(categoria);
        await _dbContext.SaveChangesAsync();

        return ApiResponse<CategoriaResponse>.Success(MapToResponse(categoria));
    }

    public async Task<ApiResponse<bool>> RemoverAsync(Guid id, string usuarioId)
    {
        try
        {
            var categoria = await _dbContext.Categorias.FindAsync(id);

            if (categoria == null)
            {
                _logger.LogWarning("Categoria com ID {Id} não encontrada para atualização", id);
                return ApiResponse<bool>.Error(ResponseType.NotFound, "Categoria não encontrada");
            }

            // Verificar se existem produtos associados a esta categoria
            var produtosAssociados = await _dbContext.Produtos
                .AnyAsync(p => p.CategoriaId == id);

            if (produtosAssociados)
                return ApiResponse<bool>.Error("Não é possível excluir a categoria pois existem produtos associados a ela");

            _dbContext.Categorias.Remove(categoria);
            await _dbContext.SaveChangesAsync();

            return ApiResponse<bool>.Success(true, "Categoria removida com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao remover categoria: {ex.Message}");
            throw;
        }
    }

    private CategoriaResponse MapToResponse(Categoria categoria)
    {
        return new CategoriaResponse
        {
            Id = categoria.Id,
            Nome = categoria.Nome,
            Descricao = categoria.Descricao,
            CatalogoId = categoria.CatalogoId,
        };
    }
}
