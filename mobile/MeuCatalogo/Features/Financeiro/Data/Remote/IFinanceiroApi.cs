using MeuCatalogo.Features.Financeiro.Data.Remote.Contracts.Responses;
using MeuCatalogo.Features.Financeiro.Domain;
using Refit;

namespace MeuCatalogo.Features.Financeiro.Data.Remote;

public interface IFinanceiroApi
{
    [Get("/financeiro/resumo")]
    Task<FinanceiroResumoResponse> ObterResumoAsync([Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Get("/financeiro/lancamentos")]
    Task<ICollection<LancamentoResponse>> ObterLancamentosAsync([Query] LancamentoTipo? tipo, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Delete("/financeiro/lancamentos/{id}")]
    Task RemoverAsync(Guid id, [Header("Authorization")] string bearerToken, CancellationToken ct = default);
}
