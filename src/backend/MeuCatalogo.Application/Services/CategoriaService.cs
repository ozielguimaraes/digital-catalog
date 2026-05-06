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
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Application.Services;

public sealed class CategoriaService : ICategoriaService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<CategoriaService> _logger;
    private readonly IMemoryCache _cache;

    public CategoriaService(ApplicationDbContext dbContext, ILogger<CategoriaService> logger, IMemoryCache cache)
    {
        _dbContext = dbContext;
        _logger = logger;
        _cache = cache;
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
            _logger.LogError(ex, "Erro ao adicionar categoria");
            throw;
        }
    }

    public async Task<ApiResponse<CategoriaResponse>> ObterPorIdAsync(string usuarioId, Guid id)
    {
        try
        {
            var categoria = await _dbContext.GetCategoriaByIdAsync(id);

            if (categoria == null)
                return ApiResponse<CategoriaResponse>.Error(ResponseType.NotFound, "Categoria não encontrada");

            return ApiResponse<CategoriaResponse>.Success(MapToResponse(categoria));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao obter categoria por ID {id} - usuário '{usuarioId}'");
            throw;
        }
    }

    public async Task<ApiResponse<IList<CategoriaResponse>>> ObterPorCatalogoAsync(Guid catalogoId, string usuarioId)
    {
        try
        {
            var cacheKey = $"categorias:catalogo:{catalogoId}";
            if (_cache.TryGetValue(cacheKey, out List<CategoriaResponse>? cachedCategorias) && cachedCategorias != null)
                return ApiResponse<IList<CategoriaResponse>>.Success(cachedCategorias);

            _logger.LogInformation($"Obter categorias do catálogo: {catalogoId}");
            var categorias = await _dbContext.ObterCategoriasPorCatalogoIdAsync(catalogoId);
            var response = categorias.Select(MapToResponse).ToList();
            _cache.Set(cacheKey, response, TimeSpan.FromSeconds(20));

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
        try
        {
            var categoria = await _dbContext.Categorias.FindAsync(id);

            if (categoria == null)
            {
                _logger.LogWarning("Categoria com ID {Id} não encontrada para atualização", id);
                return ApiResponse<CategoriaResponse>.Error(ResponseType.NotFound, "Categoria não encontrada");
            }

            categoria.Nome = request.Nome;
            categoria.Descricao = request.Descricao;
            categoria.DataAtualizacao = DateTime.UtcNow;

            _dbContext.Categorias.Update(categoria);
            await _dbContext.SaveChangesAsync();

            return ApiResponse<CategoriaResponse>.Success(MapToResponse(categoria));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao atualizar categoria com ID {id} - usuário '{usuarioId}'");
            throw;
        }
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
            bool produtosAssociados = await _dbContext.Produtos
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
