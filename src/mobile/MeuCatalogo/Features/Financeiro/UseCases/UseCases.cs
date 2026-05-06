using MeuCatalogo.Features.Financeiro.Data;
using MeuCatalogo.Features.Financeiro.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Financeiro.Domain;

namespace MeuCatalogo.Features.Financeiro.UseCases;

public sealed class GetContasUseCase(IContaRepository repo)
{
    public Task<ApiResponse<IReadOnlyList<ContaInfo>>> ExecuteAsync(bool incluirInativas = false, CancellationToken ct = default)
        => repo.ListarAsync(incluirInativas, ct);
}

public sealed class CriarContaUseCase(IContaRepository repo)
{
    public Task<ApiResponse<ContaInfo>> ExecuteAsync(ContaRequest request, CancellationToken ct = default)
        => repo.CriarAsync(request, ct);
}

public sealed class AtualizarContaUseCase(IContaRepository repo)
{
    public Task<ApiResponse<ContaInfo>> ExecuteAsync(Guid id, ContaRequest request, CancellationToken ct = default)
        => repo.AtualizarAsync(id, request, ct);
}

public sealed class RemoverContaUseCase(IContaRepository repo)
{
    public Task<ApiResponse<bool>> ExecuteAsync(Guid id, CancellationToken ct = default) => repo.RemoverAsync(id, ct);
}

public sealed class GetCategoriasFinanceirasUseCase(ICategoriaFinanceiraRepository repo)
{
    public Task<ApiResponse<IReadOnlyList<CategoriaFinanceiraInfo>>> ExecuteAsync(CategoriaFinanceiraTipo? tipo = null, CancellationToken ct = default)
        => repo.ListarAsync(tipo, ct);
}

public sealed class CriarCategoriaFinanceiraUseCase(ICategoriaFinanceiraRepository repo)
{
    public Task<ApiResponse<CategoriaFinanceiraInfo>> ExecuteAsync(CategoriaFinanceiraRequest request, CancellationToken ct = default)
        => repo.CriarAsync(request, ct);
}

public sealed class AtualizarCategoriaFinanceiraUseCase(ICategoriaFinanceiraRepository repo)
{
    public Task<ApiResponse<CategoriaFinanceiraInfo>> ExecuteAsync(Guid id, CategoriaFinanceiraRequest request, CancellationToken ct = default)
        => repo.AtualizarAsync(id, request, ct);
}

public sealed class RemoverCategoriaFinanceiraUseCase(ICategoriaFinanceiraRepository repo)
{
    public Task<ApiResponse<bool>> ExecuteAsync(Guid id, CancellationToken ct = default) => repo.RemoverAsync(id, ct);
}

public sealed class CriarSubcategoriaFinanceiraUseCase(ICategoriaFinanceiraRepository repo)
{
    public Task<ApiResponse<SubcategoriaFinanceiraInfo>> ExecuteAsync(Guid categoriaId, SubcategoriaFinanceiraRequest request, CancellationToken ct = default)
        => repo.CriarSubcategoriaAsync(categoriaId, request, ct);
}

public sealed class GetFaturaUseCase(IFaturaRepository repo)
{
    public Task<ApiResponse<FaturaInfo>> ExecuteAsync(Guid contaId, int mes, int ano, CancellationToken ct = default)
        => repo.ObterAsync(contaId, mes, ano, ct);
}

public sealed class ListarFaturasPorContaUseCase(IFaturaRepository repo)
{
    public Task<ApiResponse<IReadOnlyList<FaturaInfo>>> ExecuteAsync(Guid contaId, int? ano = null, CancellationToken ct = default)
        => repo.ListarPorContaAsync(contaId, ano, ct);
}

public sealed class GetRecorrenciasUseCase(IRecorrenciaRepository repo)
{
    public Task<ApiResponse<IReadOnlyList<RecorrenciaInfo>>> ExecuteAsync(CancellationToken ct = default) => repo.ListarAsync(ct);
}

public sealed class CriarRecorrenciaUseCase(IRecorrenciaRepository repo)
{
    public Task<ApiResponse<RecorrenciaInfo>> ExecuteAsync(RecorrenciaRequest request, CancellationToken ct = default)
        => repo.CriarAsync(request, ct);
}

public sealed class AtualizarRecorrenciaUseCase(IRecorrenciaRepository repo)
{
    public Task<ApiResponse<RecorrenciaInfo>> ExecuteAsync(Guid id, RecorrenciaRequest request, CancellationToken ct = default)
        => repo.AtualizarAsync(id, request, ct);
}

public sealed class RemoverRecorrenciaUseCase(IRecorrenciaRepository repo)
{
    public Task<ApiResponse<bool>> ExecuteAsync(Guid id, CancellationToken ct = default) => repo.RemoverAsync(id, ct);
}

public sealed class CriarTransferenciaUseCase(ITransferenciaRepository repo)
{
    public Task<ApiResponse<bool>> EntreContasAsync(TransferenciaRequest request, CancellationToken ct = default)
        => repo.CriarEntreContasAsync(request, ct);

    public Task<ApiResponse<bool>> PagamentoFaturaAsync(TransferenciaRequest request, CancellationToken ct = default)
        => repo.CriarPagamentoFaturaAsync(request, ct);
}

public sealed class GetBaixasUseCase(ILancamentoBaixaRepository repo)
{
    public Task<ApiResponse<IReadOnlyList<LancamentoBaixaInfo>>> ExecuteAsync(Guid lancamentoId, CancellationToken ct = default)
        => repo.ListarAsync(lancamentoId, ct);
}

public sealed class RegistrarBaixaUseCase(ILancamentoBaixaRepository repo)
{
    public Task<ApiResponse<LancamentoBaixaInfo>> ExecuteAsync(Guid lancamentoId, LancamentoBaixaRequest request, CancellationToken ct = default)
        => repo.CriarAsync(lancamentoId, request, ct);
}

public sealed class RemoverBaixaUseCase(ILancamentoBaixaRepository repo)
{
    public Task<ApiResponse<bool>> ExecuteAsync(Guid lancamentoId, Guid baixaId, CancellationToken ct = default)
        => repo.RemoverAsync(lancamentoId, baixaId, ct);
}

public sealed class UploadComprovanteUseCase(IComprovanteFinanceiroRepository repo)
{
    public Task<ApiResponse<ComprovanteFinanceiroInfo>> ExecuteAsync(string fileName, string contentType, Stream content, string? descricao, CancellationToken ct = default)
        => repo.EnviarAsync(fileName, contentType, content, descricao, ct);
}

public sealed class GetRelatorioPorCategoriaUseCase(IRelatorioFinanceiroRepository repo)
{
    public Task<ApiResponse<RelatorioFinanceiroInfo>> ExecuteAsync(RelatorioFinanceiroRequest request, CancellationToken ct = default)
        => repo.ObterPorCategoriaAsync(request, ct);
}

public sealed class CriarLancamentoUseCase(IFinanceiroRepository repo)
{
    public Task<ApiResponse<LancamentoInfo>> ExecuteAsync(LancamentoRequest request, CancellationToken ct = default)
        => repo.CriarLancamentoAsync(request, ct);
}

public sealed class AtualizarLancamentoUseCase(IFinanceiroRepository repo)
{
    public Task<ApiResponse<LancamentoInfo>> ExecuteAsync(Guid id, LancamentoRequest request, CancellationToken ct = default)
        => repo.AtualizarLancamentoAsync(id, request, ct);
}

public sealed class RemoverLancamentoUseCase(IFinanceiroRepository repo)
{
    public Task<ApiResponse<bool>> ExecuteAsync(Guid id, CancellationToken ct = default)
        => repo.RemoverLancamentoAsync(id, ct);
}

public sealed class GetExtratoPorContaUseCase(IExtratoRepository repo)
{
    public Task<ApiResponse<ExtratoInfo>> ExecuteAsync(Guid contaId, DateTime dataInicio, DateTime dataFim, CancellationToken ct = default)
        => repo.ObterPorContaAsync(contaId, dataInicio, dataFim, ct);
}

public sealed class GetExtratoConsolidadoUseCase(IExtratoRepository repo)
{
    public Task<ApiResponse<ExtratoInfo>> ExecuteAsync(DateTime dataInicio, DateTime dataFim, IReadOnlyList<Guid>? contaIds = null, CancellationToken ct = default)
        => repo.ObterConsolidadoAsync(dataInicio, dataFim, contaIds, ct);
}
