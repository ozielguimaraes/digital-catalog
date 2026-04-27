using MeuCatalogo.Features.Auth.Data.Local;
using MeuCatalogo.Features.Fornecedor.Data.Remote;
using MeuCatalogo.Features.Fornecedor.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Fornecedor.Data.Remote.Contracts.Responses;
using MeuCatalogo.Features.Fornecedor.Domain;
using Microsoft.Extensions.Logging;
using Refit;

namespace MeuCatalogo.Features.Fornecedor.Data;

public sealed class FornecedorRepository(
    ILogger<FornecedorRepository> logger,
    IFornecedorApi api,
    IAuthLocalDataSource authLocal) : BaseApiService, IFornecedorRepository
{
    public async Task<ApiResponse<IReadOnlyList<FornecedorInfo>>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            var token = await authLocal.GetAccessTokenAsync(ct);
            var fornecedores = await api.ObterTodosAsync($"Bearer {token}", ct);
            return ApiResponse<IReadOnlyList<FornecedorInfo>>.Success(fornecedores.Select(Map).ToList());
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao obter fornecedores.");
            return ApiResponse<IReadOnlyList<FornecedorInfo>>.Error("Erro ao obter fornecedores", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao obter fornecedores.");
            return ApiResponse<IReadOnlyList<FornecedorInfo>>.Error("Erro inesperado.");
        }
    }

    public async Task<ApiResponse<FornecedorInfo>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var token = await authLocal.GetAccessTokenAsync(ct);
            var f = await api.ObterPorIdAsync(id, $"Bearer {token}", ct);
            return ApiResponse<FornecedorInfo>.Success(Map(f));
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao obter fornecedor.");
            return ApiResponse<FornecedorInfo>.Error("Erro ao obter fornecedor", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao obter fornecedor.");
            return ApiResponse<FornecedorInfo>.Error("Erro inesperado.");
        }
    }

    public async Task<ApiResponse<FornecedorInfo>> CreateAsync(FornecedorUpsertRequest request, CancellationToken ct = default)
    {
        try
        {
            var token = await authLocal.GetAccessTokenAsync(ct);
            var created = await api.CriarAsync(request, $"Bearer {token}", ct);
            return ApiResponse<FornecedorInfo>.Success(Map(created));
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao criar fornecedor.");
            return ApiResponse<FornecedorInfo>.Error("Erro ao criar fornecedor", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao criar fornecedor.");
            return ApiResponse<FornecedorInfo>.Error("Erro inesperado.");
        }
    }

    public async Task<ApiResponse<FornecedorInfo>> UpdateAsync(Guid id, FornecedorUpsertRequest request, CancellationToken ct = default)
    {
        try
        {
            var token = await authLocal.GetAccessTokenAsync(ct);
            var updated = await api.AtualizarAsync(id, request, $"Bearer {token}", ct);
            return ApiResponse<FornecedorInfo>.Success(Map(updated));
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao atualizar fornecedor.");
            return ApiResponse<FornecedorInfo>.Error("Erro ao atualizar fornecedor", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao atualizar fornecedor.");
            return ApiResponse<FornecedorInfo>.Error("Erro inesperado.");
        }
    }

    public async Task<ApiResponse<Guid>> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var token = await authLocal.GetAccessTokenAsync(ct);
            await api.RemoverAsync(id, $"Bearer {token}", ct);
            return ApiResponse<Guid>.Success(id);
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao remover fornecedor.");
            return ApiResponse<Guid>.Error("Erro ao remover fornecedor", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao remover fornecedor.");
            return ApiResponse<Guid>.Error("Erro inesperado.");
        }
    }

    private static FornecedorInfo Map(FornecedorResponse f) => new()
    {
        Id = f.Id,
        Nome = f.Nome,
        Categoria = f.Categoria,
        NomeContato = f.NomeContato,
        Email = f.Email,
        Telefone = f.Telefone,
        Documento = f.Documento,
        Observacoes = f.Observacoes
    };
}
