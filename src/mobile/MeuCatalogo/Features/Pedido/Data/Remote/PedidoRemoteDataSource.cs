using MeuCatalogo.Features.Auth.Data.Local;
using MeuCatalogo.Features.Pedido.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Pedido.Data.Remote.Contracts.Responses;
using MeuCatalogo.Features.Pedido.Domain;
using Microsoft.Extensions.Logging;
using Refit;

namespace MeuCatalogo.Features.Pedido.Data.Remote;

public sealed class PedidoRemoteDataSource(
    ILogger<PedidoRemoteDataSource> logger,
    IPedidoApi api,
    IAuthLocalDataSource authLocal) : BaseApiService, IPedidoRemoteDataSource
{
    public async Task<ApiResponse<IReadOnlyList<PedidoInfo>>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            var token = await authLocal.GetAccessTokenAsync(ct);
            var pedidos = await api.ObterTodosAsync($"Bearer {token}", ct);
            var mapped = pedidos.Select(Map).ToList();
            return ApiResponse<IReadOnlyList<PedidoInfo>>.Success(mapped);
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao obter pedidos.");
            return ApiResponse<IReadOnlyList<PedidoInfo>>.Error("Erro ao obter pedidos", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao obter pedidos.");
            return ApiResponse<IReadOnlyList<PedidoInfo>>.Error("Erro inesperado.");
        }
    }

    public async Task<ApiResponse<PedidoInfo>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var token = await authLocal.GetAccessTokenAsync(ct);
            var pedido = await api.ObterPorIdAsync(id, $"Bearer {token}", ct);
            return ApiResponse<PedidoInfo>.Success(Map(pedido));
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao obter pedido por ID.");
            return ApiResponse<PedidoInfo>.Error("Erro ao obter pedido", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao obter pedido.");
            return ApiResponse<PedidoInfo>.Error("Erro inesperado.");
        }
    }

    public async Task<ApiResponse<IReadOnlyList<PedidoInfo>>> GetByClienteAsync(Guid clienteId, CancellationToken ct = default)
    {
        try
        {
            var token = await authLocal.GetAccessTokenAsync(ct);
            var pedidos = await api.ObterPorClienteAsync(clienteId, $"Bearer {token}", ct);
            var mapped = pedidos.Select(Map).ToList();
            return ApiResponse<IReadOnlyList<PedidoInfo>>.Success(mapped);
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao obter pedidos do cliente.");
            return ApiResponse<IReadOnlyList<PedidoInfo>>.Error("Erro ao obter pedidos do cliente", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao obter pedidos do cliente.");
            return ApiResponse<IReadOnlyList<PedidoInfo>>.Error("Erro inesperado.");
        }
    }

    public async Task<ApiResponse<PedidoInfo>> CreateAsync(PedidoCreateRequest request, CancellationToken ct = default)
    {
        try
        {
            var token = await authLocal.GetAccessTokenAsync(ct);
            var created = await api.CriarAsync(request, $"Bearer {token}", ct);
            return ApiResponse<PedidoInfo>.Success(Map(created));
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao criar pedido.");
            return ApiResponse<PedidoInfo>.Error("Erro ao criar pedido", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao criar pedido.");
            return ApiResponse<PedidoInfo>.Error("Erro inesperado.");
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
            logger.LogWarning(apiEx, "Erro ao remover pedido {Id}.", id);
            return ApiResponse<Guid>.Error("Erro ao remover pedido", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao remover pedido.");
            return ApiResponse<Guid>.Error("Erro inesperado.");
        }
    }

    private static PedidoInfo Map(PedidoResponse p) => new()
    {
        Id = p.Id,
        ClienteId = p.ClienteId,
        ClienteNome = p.ClienteNome,
        QuantidadeItens = p.Itens.Count,
        ValorTotal = p.ValorTotal,
        Status = p.Status,
        DataCriacao = p.CreatedAt
    };
}
