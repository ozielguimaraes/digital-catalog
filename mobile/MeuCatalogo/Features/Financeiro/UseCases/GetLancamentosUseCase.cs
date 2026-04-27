using MeuCatalogo.Features.Financeiro.Data;
using MeuCatalogo.Features.Financeiro.Domain;

namespace MeuCatalogo.Features.Financeiro.UseCases;

public sealed class GetLancamentosUseCase(IFinanceiroRepository repository)
{
    public Task<ApiResponse<IReadOnlyList<LancamentoInfo>>> ExecuteAsync(LancamentoTipo? tipo = null, CancellationToken ct = default)
        => repository.GetLancamentosAsync(tipo, ct);
}
