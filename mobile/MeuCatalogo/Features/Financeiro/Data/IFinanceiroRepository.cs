using MeuCatalogo.Features.Financeiro.Domain;

namespace MeuCatalogo.Features.Financeiro.Data;

public interface IFinanceiroRepository
{
    Task<ApiResponse<FinanceiroResumoInfo>> GetResumoAsync(CancellationToken ct = default);
    Task<ApiResponse<IReadOnlyList<LancamentoInfo>>> GetLancamentosAsync(LancamentoTipo? tipo = null, CancellationToken ct = default);
}
