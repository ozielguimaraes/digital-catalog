using MeuCatalogo.Features.Financeiro.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Financeiro.Domain;

namespace MeuCatalogo.Features.Financeiro.Data;

public interface IFinanceiroRepository
{
    Task<ApiResponse<FinanceiroResumoInfo>> GetResumoAsync(CancellationToken ct = default);

    Task<ApiResponse<IReadOnlyList<LancamentoInfo>>> GetLancamentosAsync(
        LancamentoTipo? tipo = null,
        LancamentoStatus? status = null,
        Guid? contaId = null,
        Guid? categoriaFinanceiraId = null,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        CancellationToken ct = default);

    Task<ApiResponse<LancamentoInfo>> GetLancamentoAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<LancamentoInfo>> CriarLancamentoAsync(LancamentoRequest request, CancellationToken ct = default);
    Task<ApiResponse<LancamentoInfo>> AtualizarLancamentoAsync(Guid id, LancamentoRequest request, CancellationToken ct = default);
    Task<ApiResponse<bool>> RemoverLancamentoAsync(Guid id, CancellationToken ct = default);
}

public interface IContaRepository
{
    Task<ApiResponse<IReadOnlyList<ContaInfo>>> ListarAsync(bool incluirInativas = false, CancellationToken ct = default);
    Task<ApiResponse<ContaInfo>> CriarAsync(ContaRequest request, CancellationToken ct = default);
    Task<ApiResponse<ContaInfo>> AtualizarAsync(Guid id, ContaRequest request, CancellationToken ct = default);
    Task<ApiResponse<bool>> RemoverAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<bool>> SetAtivoAsync(Guid id, bool ativo, CancellationToken ct = default);
}

public interface ICategoriaFinanceiraRepository
{
    Task<ApiResponse<IReadOnlyList<CategoriaFinanceiraInfo>>> ListarAsync(CategoriaFinanceiraTipo? tipo = null, CancellationToken ct = default);
    Task<ApiResponse<CategoriaFinanceiraInfo>> CriarAsync(CategoriaFinanceiraRequest request, CancellationToken ct = default);
    Task<ApiResponse<CategoriaFinanceiraInfo>> AtualizarAsync(Guid id, CategoriaFinanceiraRequest request, CancellationToken ct = default);
    Task<ApiResponse<bool>> RemoverAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<SubcategoriaFinanceiraInfo>> CriarSubcategoriaAsync(Guid categoriaId, SubcategoriaFinanceiraRequest request, CancellationToken ct = default);
}

public interface IFaturaRepository
{
    Task<ApiResponse<FaturaInfo>> ObterAsync(Guid contaId, int mes, int ano, CancellationToken ct = default);
    Task<ApiResponse<IReadOnlyList<FaturaInfo>>> ListarPorContaAsync(Guid contaId, int? ano = null, CancellationToken ct = default);
}

public interface IRecorrenciaRepository
{
    Task<ApiResponse<IReadOnlyList<RecorrenciaInfo>>> ListarAsync(CancellationToken ct = default);
    Task<ApiResponse<RecorrenciaInfo>> CriarAsync(RecorrenciaRequest request, CancellationToken ct = default);
    Task<ApiResponse<RecorrenciaInfo>> AtualizarAsync(Guid id, RecorrenciaRequest request, CancellationToken ct = default);
    Task<ApiResponse<bool>> RemoverAsync(Guid id, CancellationToken ct = default);
}

public interface ITransferenciaRepository
{
    Task<ApiResponse<bool>> CriarEntreContasAsync(TransferenciaRequest request, CancellationToken ct = default);
    Task<ApiResponse<bool>> CriarPagamentoFaturaAsync(TransferenciaRequest request, CancellationToken ct = default);
}

public interface ILancamentoBaixaRepository
{
    Task<ApiResponse<IReadOnlyList<LancamentoBaixaInfo>>> ListarAsync(Guid lancamentoId, CancellationToken ct = default);
    Task<ApiResponse<LancamentoBaixaInfo>> CriarAsync(Guid lancamentoId, LancamentoBaixaRequest request, CancellationToken ct = default);
    Task<ApiResponse<bool>> RemoverAsync(Guid lancamentoId, Guid baixaId, CancellationToken ct = default);
}

public interface IComprovanteFinanceiroRepository
{
    Task<ApiResponse<ComprovanteFinanceiroInfo>> EnviarAsync(string fileName, string contentType, Stream content, string? descricao, CancellationToken ct = default);
    Task<ApiResponse<bool>> RemoverAsync(Guid id, CancellationToken ct = default);
}

public interface IRelatorioFinanceiroRepository
{
    Task<ApiResponse<RelatorioFinanceiroInfo>> ObterPorCategoriaAsync(RelatorioFinanceiroRequest request, CancellationToken ct = default);
}

public interface IExtratoRepository
{
    Task<ApiResponse<ExtratoInfo>> ObterPorContaAsync(Guid contaId, DateTime dataInicio, DateTime dataFim, CancellationToken ct = default);
    Task<ApiResponse<ExtratoInfo>> ObterConsolidadoAsync(DateTime dataInicio, DateTime dataFim, IReadOnlyList<Guid>? contaIds, CancellationToken ct = default);
}
