using MeuCatalogo.Features.Auth.Data.Local;
using MeuCatalogo.Features.Cliente.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Cliente.Data.Remote.Contracts.Responses;
using MeuCatalogo.Features.Cliente.Domain;
using Microsoft.Extensions.Logging;
using Refit;

namespace MeuCatalogo.Features.Cliente.Data.Remote;

public sealed class ClienteRemoteDataSource(
    ILogger<ClienteRemoteDataSource> logger,
    IClienteApi api,
    IAuthLocalDataSource authLocal) : BaseApiService, IClienteRemoteDataSource
{
    public async Task<ApiResponse<IReadOnlyList<ClienteInfo>>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            var token = await authLocal.GetAccessTokenAsync(ct);
            var clientes = await api.ObterTodosAsync($"Bearer {token}", ct);
            var mapped = clientes.Select(Map).ToList();
            return ApiResponse<IReadOnlyList<ClienteInfo>>.Success(mapped);
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao obter clientes.");
            return ApiResponse<IReadOnlyList<ClienteInfo>>.Error("Erro ao obter clientes", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao obter clientes.");
            return ApiResponse<IReadOnlyList<ClienteInfo>>.Error("Erro inesperado.");
        }
    }

    public async Task<ApiResponse<ClienteInfo>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var token = await authLocal.GetAccessTokenAsync(ct);
            var cliente = await api.ObterPorIdAsync(id, $"Bearer {token}", ct);
            return ApiResponse<ClienteInfo>.Success(Map(cliente));
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao obter cliente por ID.");
            return ApiResponse<ClienteInfo>.Error("Erro ao obter cliente", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao obter cliente.");
            return ApiResponse<ClienteInfo>.Error("Erro inesperado.");
        }
    }

    public async Task<ApiResponse<ClienteInfo>> CreateAsync(ClienteUpsertRequest request, CancellationToken ct = default)
    {
        try
        {
            var token = await authLocal.GetAccessTokenAsync(ct);
            var created = await api.CriarAsync(request, $"Bearer {token}", ct);
            return ApiResponse<ClienteInfo>.Success(Map(created));
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao criar cliente.");
            return ApiResponse<ClienteInfo>.Error("Erro ao criar cliente", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao criar cliente.");
            return ApiResponse<ClienteInfo>.Error("Erro inesperado.");
        }
    }

    public async Task<ApiResponse<ClienteInfo>> UpdateAsync(Guid id, ClienteUpsertRequest request, CancellationToken ct = default)
    {
        try
        {
            var token = await authLocal.GetAccessTokenAsync(ct);
            var updated = await api.AtualizarAsync(id, request, $"Bearer {token}", ct);
            return ApiResponse<ClienteInfo>.Success(Map(updated));
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao atualizar cliente.");
            return ApiResponse<ClienteInfo>.Error("Erro ao atualizar cliente", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao atualizar cliente.");
            return ApiResponse<ClienteInfo>.Error("Erro inesperado.");
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
            logger.LogWarning(apiEx, "Erro ao remover cliente {Id}.", id);
            return ApiResponse<Guid>.Error("Erro ao remover cliente", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao remover cliente.");
            return ApiResponse<Guid>.Error("Erro inesperado.");
        }
    }

    private static ClienteInfo Map(ClienteResponse c) => new()
    {
        Id = c.Id,
        Nome = c.Nome,
        Email = c.Email,
        Telefone = c.Telefone,
        InformacoesAdicionais = c.InformacoesAdicionais
    };
}
