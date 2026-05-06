using MeuCatalogo.Features.Financeiro.Data;
using MeuCatalogo.Features.Financeiro.Domain;

namespace MeuCatalogo.Features.Financeiro.UseCases;

public sealed class GetFinanceiroResumoUseCase(IFinanceiroRepository repository)
{
    public Task<ApiResponse<FinanceiroResumoInfo>> ExecuteAsync(CancellationToken ct = default)
        => repository.GetResumoAsync(ct);
}
