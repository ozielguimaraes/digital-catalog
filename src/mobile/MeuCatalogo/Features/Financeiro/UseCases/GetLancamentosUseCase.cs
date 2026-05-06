using MeuCatalogo.Features.Financeiro.Data;
using MeuCatalogo.Features.Financeiro.Domain;

namespace MeuCatalogo.Features.Financeiro.UseCases;

public sealed class GetLancamentosUseCase(IFinanceiroRepository repository)
{
    public Task<ApiResponse<IReadOnlyList<LancamentoInfo>>> ExecuteAsync(
        LancamentoTipo? tipo = null,
        LancamentoStatus? status = null,
        Guid? contaId = null,
        Guid? categoriaFinanceiraId = null,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        CancellationToken ct = default)
        => repository.GetLancamentosAsync(tipo, status, contaId, categoriaFinanceiraId, dataInicio, dataFim, ct);
}
