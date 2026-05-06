using MeuCatalogo.Features.Auth.Data.Local;
using MeuCatalogo.Features.Financeiro.Data.Remote;
using MeuCatalogo.Features.Financeiro.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Financeiro.Domain;
using Microsoft.Extensions.Logging;
using Refit;

namespace MeuCatalogo.Features.Financeiro.Data;

public sealed class ContaRepository(
    ILogger<ContaRepository> logger, IFinanceiroApi api, IAuthLocalDataSource auth) : BaseApiService, IContaRepository
{
    public async Task<ApiResponse<IReadOnlyList<ContaInfo>>> ListarAsync(bool incluirInativas = false, CancellationToken ct = default)
    {
        try
        {
            var token = await auth.GetAccessTokenAsync(ct);
            var lista = await api.ObterContasAsync(incluirInativas, $"Bearer {token}", ct);
            return ApiResponse<IReadOnlyList<ContaInfo>>.Success(lista.Select(FinanceiroMapper.MapConta).ToList());
        }
        catch (ApiException ex) { return ApiResponse<IReadOnlyList<ContaInfo>>.Error("Erro ao listar contas", GetProblemDetails(ex)); }
        catch (Exception ex) { logger.LogError(ex, "Erro listar contas"); return ApiResponse<IReadOnlyList<ContaInfo>>.Error("Erro inesperado"); }
    }

    public async Task<ApiResponse<ContaInfo>> CriarAsync(ContaRequest request, CancellationToken ct = default)
    {
        try
        {
            var token = await auth.GetAccessTokenAsync(ct);
            var c = await api.CriarContaAsync(request, $"Bearer {token}", ct);
            return ApiResponse<ContaInfo>.Success(FinanceiroMapper.MapConta(c));
        }
        catch (ApiException ex) { return ApiResponse<ContaInfo>.Error("Erro ao criar conta", GetProblemDetails(ex)); }
        catch (Exception ex) { logger.LogError(ex, "Erro criar conta"); return ApiResponse<ContaInfo>.Error("Erro inesperado"); }
    }

    public async Task<ApiResponse<ContaInfo>> AtualizarAsync(Guid id, ContaRequest request, CancellationToken ct = default)
    {
        try
        {
            var token = await auth.GetAccessTokenAsync(ct);
            var c = await api.AtualizarContaAsync(id, request, $"Bearer {token}", ct);
            return ApiResponse<ContaInfo>.Success(FinanceiroMapper.MapConta(c));
        }
        catch (ApiException ex) { return ApiResponse<ContaInfo>.Error("Erro ao atualizar conta", GetProblemDetails(ex)); }
        catch (Exception ex) { logger.LogError(ex, "Erro atualizar conta"); return ApiResponse<ContaInfo>.Error("Erro inesperado"); }
    }

    public async Task<ApiResponse<bool>> RemoverAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var token = await auth.GetAccessTokenAsync(ct);
            await api.RemoverContaAsync(id, $"Bearer {token}", ct);
            return ApiResponse<bool>.Success(true);
        }
        catch (ApiException ex) { return ApiResponse<bool>.Error("Erro ao remover conta", GetProblemDetails(ex)); }
        catch (Exception ex) { logger.LogError(ex, "Erro remover conta"); return ApiResponse<bool>.Error("Erro inesperado"); }
    }

    public async Task<ApiResponse<bool>> SetAtivoAsync(Guid id, bool ativo, CancellationToken ct = default)
    {
        try
        {
            var token = await auth.GetAccessTokenAsync(ct);
            if (ativo) await api.AtivarContaAsync(id, $"Bearer {token}", ct);
            else await api.InativarContaAsync(id, $"Bearer {token}", ct);
            return ApiResponse<bool>.Success(true);
        }
        catch (ApiException ex) { return ApiResponse<bool>.Error("Erro ao alterar status", GetProblemDetails(ex)); }
        catch (Exception ex) { logger.LogError(ex, "Erro set ativo conta"); return ApiResponse<bool>.Error("Erro inesperado"); }
    }
}

public sealed class CategoriaFinanceiraRepository(
    ILogger<CategoriaFinanceiraRepository> logger, IFinanceiroApi api, IAuthLocalDataSource auth) : BaseApiService, ICategoriaFinanceiraRepository
{
    public async Task<ApiResponse<IReadOnlyList<CategoriaFinanceiraInfo>>> ListarAsync(CategoriaFinanceiraTipo? tipo = null, CancellationToken ct = default)
    {
        try
        {
            var token = await auth.GetAccessTokenAsync(ct);
            var lista = await api.ObterCategoriasAsync(tipo, $"Bearer {token}", ct);
            return ApiResponse<IReadOnlyList<CategoriaFinanceiraInfo>>.Success(lista.Select(FinanceiroMapper.MapCategoria).ToList());
        }
        catch (ApiException ex) { return ApiResponse<IReadOnlyList<CategoriaFinanceiraInfo>>.Error("Erro ao listar categorias", GetProblemDetails(ex)); }
        catch (Exception ex) { logger.LogError(ex, "Erro listar categorias"); return ApiResponse<IReadOnlyList<CategoriaFinanceiraInfo>>.Error("Erro inesperado"); }
    }

    public async Task<ApiResponse<CategoriaFinanceiraInfo>> CriarAsync(CategoriaFinanceiraRequest request, CancellationToken ct = default)
    {
        try
        {
            var token = await auth.GetAccessTokenAsync(ct);
            var c = await api.CriarCategoriaAsync(request, $"Bearer {token}", ct);
            return ApiResponse<CategoriaFinanceiraInfo>.Success(FinanceiroMapper.MapCategoria(c));
        }
        catch (ApiException ex) { return ApiResponse<CategoriaFinanceiraInfo>.Error("Erro ao criar categoria", GetProblemDetails(ex)); }
        catch (Exception ex) { logger.LogError(ex, "Erro criar categoria"); return ApiResponse<CategoriaFinanceiraInfo>.Error("Erro inesperado"); }
    }

    public async Task<ApiResponse<CategoriaFinanceiraInfo>> AtualizarAsync(Guid id, CategoriaFinanceiraRequest request, CancellationToken ct = default)
    {
        try
        {
            var token = await auth.GetAccessTokenAsync(ct);
            var c = await api.AtualizarCategoriaAsync(id, request, $"Bearer {token}", ct);
            return ApiResponse<CategoriaFinanceiraInfo>.Success(FinanceiroMapper.MapCategoria(c));
        }
        catch (ApiException ex) { return ApiResponse<CategoriaFinanceiraInfo>.Error("Erro ao atualizar categoria", GetProblemDetails(ex)); }
        catch (Exception ex) { logger.LogError(ex, "Erro atualizar categoria"); return ApiResponse<CategoriaFinanceiraInfo>.Error("Erro inesperado"); }
    }

    public async Task<ApiResponse<bool>> RemoverAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var token = await auth.GetAccessTokenAsync(ct);
            await api.RemoverCategoriaAsync(id, $"Bearer {token}", ct);
            return ApiResponse<bool>.Success(true);
        }
        catch (ApiException ex) { return ApiResponse<bool>.Error("Erro ao remover categoria", GetProblemDetails(ex)); }
        catch (Exception ex) { logger.LogError(ex, "Erro remover categoria"); return ApiResponse<bool>.Error("Erro inesperado"); }
    }

    public async Task<ApiResponse<SubcategoriaFinanceiraInfo>> CriarSubcategoriaAsync(Guid categoriaId, SubcategoriaFinanceiraRequest request, CancellationToken ct = default)
    {
        try
        {
            var token = await auth.GetAccessTokenAsync(ct);
            var s = await api.CriarSubcategoriaAsync(categoriaId, request, $"Bearer {token}", ct);
            return ApiResponse<SubcategoriaFinanceiraInfo>.Success(FinanceiroMapper.MapSub(s));
        }
        catch (ApiException ex) { return ApiResponse<SubcategoriaFinanceiraInfo>.Error("Erro ao criar subcategoria", GetProblemDetails(ex)); }
        catch (Exception ex) { logger.LogError(ex, "Erro criar subcategoria"); return ApiResponse<SubcategoriaFinanceiraInfo>.Error("Erro inesperado"); }
    }
}

public sealed class FaturaRepository(
    ILogger<FaturaRepository> logger, IFinanceiroApi api, IAuthLocalDataSource auth) : BaseApiService, IFaturaRepository
{
    public async Task<ApiResponse<FaturaInfo>> ObterAsync(Guid contaId, int mes, int ano, CancellationToken ct = default)
    {
        try
        {
            var token = await auth.GetAccessTokenAsync(ct);
            var f = await api.ObterFaturaAsync(contaId, mes, ano, $"Bearer {token}", ct);
            return ApiResponse<FaturaInfo>.Success(FinanceiroMapper.MapFatura(f));
        }
        catch (ApiException ex) { return ApiResponse<FaturaInfo>.Error("Erro ao obter fatura", GetProblemDetails(ex)); }
        catch (Exception ex) { logger.LogError(ex, "Erro obter fatura"); return ApiResponse<FaturaInfo>.Error("Erro inesperado"); }
    }

    public async Task<ApiResponse<IReadOnlyList<FaturaInfo>>> ListarPorContaAsync(Guid contaId, int? ano = null, CancellationToken ct = default)
    {
        try
        {
            var token = await auth.GetAccessTokenAsync(ct);
            var lista = await api.ListarFaturasPorContaAsync(contaId, ano, $"Bearer {token}", ct);
            return ApiResponse<IReadOnlyList<FaturaInfo>>.Success(lista.Select(FinanceiroMapper.MapFatura).ToList());
        }
        catch (ApiException ex) { return ApiResponse<IReadOnlyList<FaturaInfo>>.Error("Erro ao listar faturas", GetProblemDetails(ex)); }
        catch (Exception ex) { logger.LogError(ex, "Erro listar faturas"); return ApiResponse<IReadOnlyList<FaturaInfo>>.Error("Erro inesperado"); }
    }
}

public sealed class RecorrenciaRepository(
    ILogger<RecorrenciaRepository> logger, IFinanceiroApi api, IAuthLocalDataSource auth) : BaseApiService, IRecorrenciaRepository
{
    public async Task<ApiResponse<IReadOnlyList<RecorrenciaInfo>>> ListarAsync(CancellationToken ct = default)
    {
        try
        {
            var token = await auth.GetAccessTokenAsync(ct);
            var lista = await api.ObterRecorrenciasAsync($"Bearer {token}", ct);
            return ApiResponse<IReadOnlyList<RecorrenciaInfo>>.Success(lista.Select(FinanceiroMapper.MapRecorrencia).ToList());
        }
        catch (ApiException ex) { return ApiResponse<IReadOnlyList<RecorrenciaInfo>>.Error("Erro ao listar recorrências", GetProblemDetails(ex)); }
        catch (Exception ex) { logger.LogError(ex, "Erro listar recorrências"); return ApiResponse<IReadOnlyList<RecorrenciaInfo>>.Error("Erro inesperado"); }
    }

    public async Task<ApiResponse<RecorrenciaInfo>> CriarAsync(RecorrenciaRequest request, CancellationToken ct = default)
    {
        try
        {
            var token = await auth.GetAccessTokenAsync(ct);
            var r = await api.CriarRecorrenciaAsync(request, $"Bearer {token}", ct);
            return ApiResponse<RecorrenciaInfo>.Success(FinanceiroMapper.MapRecorrencia(r));
        }
        catch (ApiException ex) { return ApiResponse<RecorrenciaInfo>.Error("Erro ao criar recorrência", GetProblemDetails(ex)); }
        catch (Exception ex) { logger.LogError(ex, "Erro criar recorrência"); return ApiResponse<RecorrenciaInfo>.Error("Erro inesperado"); }
    }

    public async Task<ApiResponse<RecorrenciaInfo>> AtualizarAsync(Guid id, RecorrenciaRequest request, CancellationToken ct = default)
    {
        try
        {
            var token = await auth.GetAccessTokenAsync(ct);
            var r = await api.AtualizarRecorrenciaAsync(id, request, $"Bearer {token}", ct);
            return ApiResponse<RecorrenciaInfo>.Success(FinanceiroMapper.MapRecorrencia(r));
        }
        catch (ApiException ex) { return ApiResponse<RecorrenciaInfo>.Error("Erro ao atualizar recorrência", GetProblemDetails(ex)); }
        catch (Exception ex) { logger.LogError(ex, "Erro atualizar recorrência"); return ApiResponse<RecorrenciaInfo>.Error("Erro inesperado"); }
    }

    public async Task<ApiResponse<bool>> RemoverAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var token = await auth.GetAccessTokenAsync(ct);
            await api.RemoverRecorrenciaAsync(id, $"Bearer {token}", ct);
            return ApiResponse<bool>.Success(true);
        }
        catch (ApiException ex) { return ApiResponse<bool>.Error("Erro ao remover recorrência", GetProblemDetails(ex)); }
        catch (Exception ex) { logger.LogError(ex, "Erro remover recorrência"); return ApiResponse<bool>.Error("Erro inesperado"); }
    }
}

public sealed class TransferenciaRepository(
    ILogger<TransferenciaRepository> logger, IFinanceiroApi api, IAuthLocalDataSource auth) : BaseApiService, ITransferenciaRepository
{
    public async Task<ApiResponse<bool>> CriarEntreContasAsync(TransferenciaRequest request, CancellationToken ct = default)
    {
        try
        {
            var token = await auth.GetAccessTokenAsync(ct);
            await api.CriarTransferenciaEntreContasAsync(request, $"Bearer {token}", ct);
            return ApiResponse<bool>.Success(true);
        }
        catch (ApiException ex) { return ApiResponse<bool>.Error("Erro ao criar transferência", GetProblemDetails(ex)); }
        catch (Exception ex) { logger.LogError(ex, "Erro transferência"); return ApiResponse<bool>.Error("Erro inesperado"); }
    }

    public async Task<ApiResponse<bool>> CriarPagamentoFaturaAsync(TransferenciaRequest request, CancellationToken ct = default)
    {
        try
        {
            var token = await auth.GetAccessTokenAsync(ct);
            await api.CriarPagamentoFaturaAsync(request, $"Bearer {token}", ct);
            return ApiResponse<bool>.Success(true);
        }
        catch (ApiException ex) { return ApiResponse<bool>.Error("Erro ao registrar pagamento de fatura", GetProblemDetails(ex)); }
        catch (Exception ex) { logger.LogError(ex, "Erro pagamento fatura"); return ApiResponse<bool>.Error("Erro inesperado"); }
    }
}

public sealed class LancamentoBaixaRepository(
    ILogger<LancamentoBaixaRepository> logger, IFinanceiroApi api, IAuthLocalDataSource auth) : BaseApiService, ILancamentoBaixaRepository
{
    public async Task<ApiResponse<IReadOnlyList<LancamentoBaixaInfo>>> ListarAsync(Guid lancamentoId, CancellationToken ct = default)
    {
        try
        {
            var token = await auth.GetAccessTokenAsync(ct);
            var lista = await api.ObterBaixasAsync(lancamentoId, $"Bearer {token}", ct);
            return ApiResponse<IReadOnlyList<LancamentoBaixaInfo>>.Success(lista.Select(FinanceiroMapper.MapBaixa).ToList());
        }
        catch (ApiException ex) { return ApiResponse<IReadOnlyList<LancamentoBaixaInfo>>.Error("Erro ao listar baixas", GetProblemDetails(ex)); }
        catch (Exception ex) { logger.LogError(ex, "Erro listar baixas"); return ApiResponse<IReadOnlyList<LancamentoBaixaInfo>>.Error("Erro inesperado"); }
    }

    public async Task<ApiResponse<LancamentoBaixaInfo>> CriarAsync(Guid lancamentoId, LancamentoBaixaRequest request, CancellationToken ct = default)
    {
        try
        {
            var token = await auth.GetAccessTokenAsync(ct);
            var b = await api.CriarBaixaAsync(lancamentoId, request, $"Bearer {token}", ct);
            return ApiResponse<LancamentoBaixaInfo>.Success(FinanceiroMapper.MapBaixa(b));
        }
        catch (ApiException ex) { return ApiResponse<LancamentoBaixaInfo>.Error("Erro ao registrar baixa", GetProblemDetails(ex)); }
        catch (Exception ex) { logger.LogError(ex, "Erro criar baixa"); return ApiResponse<LancamentoBaixaInfo>.Error("Erro inesperado"); }
    }

    public async Task<ApiResponse<bool>> RemoverAsync(Guid lancamentoId, Guid baixaId, CancellationToken ct = default)
    {
        try
        {
            var token = await auth.GetAccessTokenAsync(ct);
            await api.RemoverBaixaAsync(lancamentoId, baixaId, $"Bearer {token}", ct);
            return ApiResponse<bool>.Success(true);
        }
        catch (ApiException ex) { return ApiResponse<bool>.Error("Erro ao remover baixa", GetProblemDetails(ex)); }
        catch (Exception ex) { logger.LogError(ex, "Erro remover baixa"); return ApiResponse<bool>.Error("Erro inesperado"); }
    }
}

public sealed class ComprovanteFinanceiroRepository(
    ILogger<ComprovanteFinanceiroRepository> logger, IFinanceiroApi api, IAuthLocalDataSource auth) : BaseApiService, IComprovanteFinanceiroRepository
{
    public async Task<ApiResponse<ComprovanteFinanceiroInfo>> EnviarAsync(string fileName, string contentType, Stream content, string? descricao, CancellationToken ct = default)
    {
        try
        {
            var token = await auth.GetAccessTokenAsync(ct);
            var part = new StreamPart(content, fileName, contentType);
            var r = await api.EnviarComprovanteAsync(part, descricao, $"Bearer {token}", ct);
            return ApiResponse<ComprovanteFinanceiroInfo>.Success(FinanceiroMapper.MapComprovante(r));
        }
        catch (ApiException ex) { return ApiResponse<ComprovanteFinanceiroInfo>.Error("Erro ao enviar comprovante", GetProblemDetails(ex)); }
        catch (Exception ex) { logger.LogError(ex, "Erro upload comprovante"); return ApiResponse<ComprovanteFinanceiroInfo>.Error("Erro inesperado"); }
    }

    public async Task<ApiResponse<bool>> RemoverAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var token = await auth.GetAccessTokenAsync(ct);
            await api.RemoverComprovanteAsync(id, $"Bearer {token}", ct);
            return ApiResponse<bool>.Success(true);
        }
        catch (ApiException ex) { return ApiResponse<bool>.Error("Erro ao remover comprovante", GetProblemDetails(ex)); }
        catch (Exception ex) { logger.LogError(ex, "Erro remover comprovante"); return ApiResponse<bool>.Error("Erro inesperado"); }
    }
}

public sealed class RelatorioFinanceiroRepository(
    ILogger<RelatorioFinanceiroRepository> logger, IFinanceiroApi api, IAuthLocalDataSource auth) : BaseApiService, IRelatorioFinanceiroRepository
{
    public async Task<ApiResponse<RelatorioFinanceiroInfo>> ObterPorCategoriaAsync(RelatorioFinanceiroRequest request, CancellationToken ct = default)
    {
        try
        {
            var token = await auth.GetAccessTokenAsync(ct);
            var r = await api.ObterRelatorioPorCategoriaAsync(request, $"Bearer {token}", ct);
            return ApiResponse<RelatorioFinanceiroInfo>.Success(FinanceiroMapper.MapRelatorio(r));
        }
        catch (ApiException ex) { return ApiResponse<RelatorioFinanceiroInfo>.Error("Erro ao gerar relatório", GetProblemDetails(ex)); }
        catch (Exception ex) { logger.LogError(ex, "Erro relatório"); return ApiResponse<RelatorioFinanceiroInfo>.Error("Erro inesperado"); }
    }
}

public sealed class ExtratoRepository(
    ILogger<ExtratoRepository> logger, IFinanceiroApi api, IAuthLocalDataSource auth) : BaseApiService, IExtratoRepository
{
    public async Task<ApiResponse<ExtratoInfo>> ObterPorContaAsync(Guid contaId, DateTime dataInicio, DateTime dataFim, CancellationToken ct = default)
    {
        try
        {
            var token = await auth.GetAccessTokenAsync(ct);
            var r = await api.ObterExtratoPorContaAsync(contaId, dataInicio, dataFim, $"Bearer {token}", ct);
            return ApiResponse<ExtratoInfo>.Success(FinanceiroMapper.MapExtrato(r));
        }
        catch (ApiException ex) { return ApiResponse<ExtratoInfo>.Error("Erro ao obter extrato", GetProblemDetails(ex)); }
        catch (Exception ex) { logger.LogError(ex, "Erro extrato"); return ApiResponse<ExtratoInfo>.Error("Erro inesperado"); }
    }

    public async Task<ApiResponse<ExtratoInfo>> ObterConsolidadoAsync(DateTime dataInicio, DateTime dataFim, IReadOnlyList<Guid>? contaIds, CancellationToken ct = default)
    {
        try
        {
            var token = await auth.GetAccessTokenAsync(ct);
            var r = await api.ObterExtratoConsolidadoAsync(dataInicio, dataFim, contaIds?.ToList(), $"Bearer {token}", ct);
            return ApiResponse<ExtratoInfo>.Success(FinanceiroMapper.MapExtrato(r));
        }
        catch (ApiException ex) { return ApiResponse<ExtratoInfo>.Error("Erro ao obter extrato consolidado", GetProblemDetails(ex)); }
        catch (Exception ex) { logger.LogError(ex, "Erro extrato consolidado"); return ApiResponse<ExtratoInfo>.Error("Erro inesperado"); }
    }
}
