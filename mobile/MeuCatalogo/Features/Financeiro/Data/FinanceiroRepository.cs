using MeuCatalogo.Features.Auth.Data.Local;
using MeuCatalogo.Features.Financeiro.Data.Remote;
using MeuCatalogo.Features.Financeiro.Data.Remote.Contracts.Responses;
using MeuCatalogo.Features.Financeiro.Domain;
using Microsoft.Extensions.Logging;
using Refit;

namespace MeuCatalogo.Features.Financeiro.Data;

public sealed class FinanceiroRepository(
    ILogger<FinanceiroRepository> logger,
    IFinanceiroApi api,
    IAuthLocalDataSource authLocal) : BaseApiService, IFinanceiroRepository
{
    public async Task<ApiResponse<FinanceiroResumoInfo>> GetResumoAsync(CancellationToken ct = default)
    {
        try
        {
            var token = await authLocal.GetAccessTokenAsync(ct);
            var resumo = await api.ObterResumoAsync($"Bearer {token}", ct);
            return ApiResponse<FinanceiroResumoInfo>.Success(MapResumo(resumo));
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao obter resumo financeiro.");
            return ApiResponse<FinanceiroResumoInfo>.Error("Erro ao obter resumo financeiro", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao obter resumo financeiro.");
            return ApiResponse<FinanceiroResumoInfo>.Error("Erro inesperado.");
        }
    }

    public async Task<ApiResponse<IReadOnlyList<LancamentoInfo>>> GetLancamentosAsync(LancamentoTipo? tipo = null, CancellationToken ct = default)
    {
        try
        {
            var token = await authLocal.GetAccessTokenAsync(ct);
            var lancamentos = await api.ObterLancamentosAsync(tipo, $"Bearer {token}", ct);
            return ApiResponse<IReadOnlyList<LancamentoInfo>>.Success(lancamentos.Select(Map).ToList());
        }
        catch (ApiException apiEx)
        {
            logger.LogWarning(apiEx, "Erro ao obter lançamentos.");
            return ApiResponse<IReadOnlyList<LancamentoInfo>>.Error("Erro ao obter lançamentos", GetProblemDetails(apiEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao obter lançamentos.");
            return ApiResponse<IReadOnlyList<LancamentoInfo>>.Error("Erro inesperado.");
        }
    }

    private static LancamentoInfo Map(LancamentoResponse l) => new()
    {
        Id = l.Id,
        Descricao = l.Descricao,
        Valor = l.Valor,
        DataVencimento = l.DataVencimento,
        DataPagamento = l.DataPagamento,
        Tipo = l.Tipo,
        Status = l.Status,
        FornecedorNome = l.FornecedorNome,
        PedidoId = l.PedidoId
    };

    private static FinanceiroResumoInfo MapResumo(FinanceiroResumoResponse r) => new()
    {
        TotalAReceber = r.TotalAReceber,
        TotalAPagar = r.TotalAPagar,
        SaldoPrevisto = r.SaldoPrevisto,
        RecebidoNoMes = r.RecebidoNoMes,
        PagoNoMes = r.PagoNoMes,
        PeriodoLabel = r.PeriodoLabel
    };
}
