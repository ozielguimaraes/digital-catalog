using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Infrastructure.Data;
using MeuCatalogo.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeuCatalogo.Application.Services;

public sealed class FornecedorService : IFornecedorService
{
    private readonly ApplicationDbContext _dbContext;

    public FornecedorService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApiResponse<List<FornecedorResponse>>> GetAllAsync(string userId)
    {
        try
        {
            var fornecedores = await _dbContext.Fornecedores
                .Where(f => f.UserId == userId && f.Ativo)
                .OrderBy(f => f.Nome)
                .ToListAsync();

            return ApiResponse<List<FornecedorResponse>>.Success(fornecedores.Select(MapToResponse).ToList());
        }
        catch (Exception ex)
        {
            return ApiResponse<List<FornecedorResponse>>.Error($"Erro ao buscar fornecedores: {ex.Message}");
        }
    }

    public async Task<ApiResponse<FornecedorResponse>> GetByIdAsync(Guid id, string userId)
    {
        try
        {
            var fornecedor = await _dbContext.Fornecedores
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

            if (fornecedor == null)
                return ApiResponse<FornecedorResponse>.Error("Fornecedor não encontrado");

            return ApiResponse<FornecedorResponse>.Success(MapToResponse(fornecedor));
        }
        catch (Exception ex)
        {
            return ApiResponse<FornecedorResponse>.Error($"Erro ao buscar fornecedor: {ex.Message}");
        }
    }

    public async Task<ApiResponse<FornecedorResponse>> CreateAsync(FornecedorRequest request, string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Nome))
                return ApiResponse<FornecedorResponse>.Error("Nome do fornecedor é obrigatório");

            var fornecedor = new Fornecedor
            {
                Nome = request.Nome,
                Categoria = request.Categoria,
                NomeContato = request.NomeContato,
                Email = request.Email,
                Telefone = request.Telefone,
                Documento = request.Documento,
                Observacoes = request.Observacoes,
                UserId = userId
            };

            await _dbContext.Fornecedores.AddAsync(fornecedor);
            await _dbContext.SaveChangesAsync();

            return ApiResponse<FornecedorResponse>.Success(MapToResponse(fornecedor));
        }
        catch (Exception ex)
        {
            return ApiResponse<FornecedorResponse>.Error($"Erro ao criar fornecedor: {ex.Message}");
        }
    }

    public async Task<ApiResponse<FornecedorResponse>> UpdateAsync(Guid id, FornecedorRequest request, string userId)
    {
        try
        {
            var fornecedor = await _dbContext.Fornecedores
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

            if (fornecedor == null)
                return ApiResponse<FornecedorResponse>.Error("Fornecedor não encontrado");

            fornecedor.Nome = request.Nome;
            fornecedor.Categoria = request.Categoria;
            fornecedor.NomeContato = request.NomeContato;
            fornecedor.Email = request.Email;
            fornecedor.Telefone = request.Telefone;
            fornecedor.Documento = request.Documento;
            fornecedor.Observacoes = request.Observacoes;
            fornecedor.DataAtualizacao = DateTime.UtcNow;

            _dbContext.Fornecedores.Update(fornecedor);
            await _dbContext.SaveChangesAsync();

            return ApiResponse<FornecedorResponse>.Success(MapToResponse(fornecedor));
        }
        catch (Exception ex)
        {
            return ApiResponse<FornecedorResponse>.Error($"Erro ao atualizar fornecedor: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, string userId)
    {
        try
        {
            var fornecedor = await _dbContext.Fornecedores
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

            if (fornecedor == null)
                return ApiResponse<bool>.Error("Fornecedor não encontrado");

            _dbContext.Fornecedores.Remove(fornecedor);
            await _dbContext.SaveChangesAsync();

            return ApiResponse<bool>.Success(true, "Fornecedor removido com sucesso");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Error($"Erro ao remover fornecedor: {ex.Message}");
        }
    }

    private static FornecedorResponse MapToResponse(Fornecedor f) => new()
    {
        Id = f.Id,
        Nome = f.Nome,
        Categoria = f.Categoria,
        NomeContato = f.NomeContato,
        Email = f.Email,
        Telefone = f.Telefone,
        Documento = f.Documento,
        Observacoes = f.Observacoes,
        CreatedAt = f.DataCriacao,
        UpdatedAt = f.DataAtualizacao
    };
}
