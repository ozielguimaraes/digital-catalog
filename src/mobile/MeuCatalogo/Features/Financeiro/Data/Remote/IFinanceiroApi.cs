using MeuCatalogo.Features.Financeiro.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Financeiro.Data.Remote.Contracts.Responses;
using MeuCatalogo.Features.Financeiro.Domain;
using Refit;

namespace MeuCatalogo.Features.Financeiro.Data.Remote;

public interface IFinanceiroApi
{
    [Get("/financeiro/resumo")]
    Task<FinanceiroResumoResponse> ObterResumoAsync([Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Get("/financeiro/lancamentos")]
    Task<ICollection<LancamentoResponse>> ObterLancamentosAsync(
        [Query] LancamentoTipo? tipo,
        [Query] LancamentoStatus? status,
        [Query] Guid? contaId,
        [Query] Guid? categoriaFinanceiraId,
        [Query] DateTime? dataInicio,
        [Query] DateTime? dataFim,
        [Header("Authorization")] string bearerToken,
        CancellationToken ct = default);

    [Get("/financeiro/lancamentos/{id}")]
    Task<LancamentoResponse> ObterLancamentoAsync(Guid id, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Post("/financeiro/lancamentos")]
    Task<LancamentoResponse> CriarLancamentoAsync([Body] LancamentoRequest request, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Put("/financeiro/lancamentos/{id}")]
    Task<LancamentoResponse> AtualizarLancamentoAsync(Guid id, [Body] LancamentoRequest request, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Delete("/financeiro/lancamentos/{id}")]
    Task RemoverAsync(Guid id, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    // Contas
    [Get("/contas")]
    Task<ICollection<ContaResponse>> ObterContasAsync([Query] bool incluirInativas, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Post("/contas")]
    Task<ContaResponse> CriarContaAsync([Body] ContaRequest request, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Put("/contas/{id}")]
    Task<ContaResponse> AtualizarContaAsync(Guid id, [Body] ContaRequest request, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Delete("/contas/{id}")]
    Task RemoverContaAsync(Guid id, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Post("/contas/{id}/inativar")]
    Task InativarContaAsync(Guid id, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Post("/contas/{id}/ativar")]
    Task AtivarContaAsync(Guid id, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    // Categorias financeiras
    [Get("/categorias-financeiras")]
    Task<ICollection<CategoriaFinanceiraResponse>> ObterCategoriasAsync([Query] CategoriaFinanceiraTipo? tipo, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Post("/categorias-financeiras")]
    Task<CategoriaFinanceiraResponse> CriarCategoriaAsync([Body] CategoriaFinanceiraRequest request, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Put("/categorias-financeiras/{id}")]
    Task<CategoriaFinanceiraResponse> AtualizarCategoriaAsync(Guid id, [Body] CategoriaFinanceiraRequest request, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Delete("/categorias-financeiras/{id}")]
    Task RemoverCategoriaAsync(Guid id, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Post("/categorias-financeiras/{id}/subcategorias")]
    Task<SubcategoriaFinanceiraResponse> CriarSubcategoriaAsync(Guid id, [Body] SubcategoriaFinanceiraRequest request, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    // Transferências
    [Post("/transferencias/entre-contas")]
    Task<ICollection<LancamentoResponse>> CriarTransferenciaEntreContasAsync([Body] TransferenciaRequest request, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Post("/transferencias/pagamento-fatura")]
    Task<ICollection<LancamentoResponse>> CriarPagamentoFaturaAsync([Body] TransferenciaRequest request, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    // Faturas
    [Get("/faturas")]
    Task<FaturaResponse> ObterFaturaAsync([Query] Guid contaId, [Query] int mes, [Query] int ano, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Get("/faturas/conta/{contaId}")]
    Task<ICollection<FaturaResponse>> ListarFaturasPorContaAsync(Guid contaId, [Query] int? ano, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    // Recorrências
    [Get("/recorrencias")]
    Task<ICollection<RecorrenciaResponse>> ObterRecorrenciasAsync([Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Post("/recorrencias")]
    Task<RecorrenciaResponse> CriarRecorrenciaAsync([Body] RecorrenciaRequest request, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Put("/recorrencias/{id}")]
    Task<RecorrenciaResponse> AtualizarRecorrenciaAsync(Guid id, [Body] RecorrenciaRequest request, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Delete("/recorrencias/{id}")]
    Task RemoverRecorrenciaAsync(Guid id, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    // Baixas
    [Get("/lancamentos/{lancamentoId}/baixas")]
    Task<ICollection<LancamentoBaixaResponse>> ObterBaixasAsync(Guid lancamentoId, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Post("/lancamentos/{lancamentoId}/baixas")]
    Task<LancamentoBaixaResponse> CriarBaixaAsync(Guid lancamentoId, [Body] LancamentoBaixaRequest request, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Delete("/lancamentos/{lancamentoId}/baixas/{baixaId}")]
    Task RemoverBaixaAsync(Guid lancamentoId, Guid baixaId, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    // Comprovantes
    [Multipart]
    [Post("/comprovantes-financeiros")]
    Task<ComprovanteFinanceiroResponse> EnviarComprovanteAsync(StreamPart arquivo, [AliasAs("descricao")] string? descricao, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Delete("/comprovantes-financeiros/{id}")]
    Task RemoverComprovanteAsync(Guid id, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    // Relatórios
    [Post("/relatorios-financeiros/lancamentos-por-categoria")]
    Task<RelatorioFinanceiroResponse> ObterRelatorioPorCategoriaAsync([Body] RelatorioFinanceiroRequest request, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    // Extrato
    [Get("/contas/{contaId}/extrato")]
    Task<ExtratoResponse> ObterExtratoPorContaAsync(Guid contaId, [Query] DateTime dataInicio, [Query] DateTime dataFim, [Header("Authorization")] string bearerToken, CancellationToken ct = default);

    [Get("/contas/extrato")]
    Task<ExtratoResponse> ObterExtratoConsolidadoAsync([Query] DateTime dataInicio, [Query] DateTime dataFim, [Query(CollectionFormat.Multi)] List<Guid>? contaIds, [Header("Authorization")] string bearerToken, CancellationToken ct = default);
}
