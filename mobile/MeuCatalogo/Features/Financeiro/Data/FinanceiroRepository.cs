using MeuCatalogo.Features.Auth.Data.Local;
using MeuCatalogo.Features.Financeiro.Data.Remote;
using MeuCatalogo.Features.Financeiro.Data.Remote.Contracts.Requests;
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
            return ApiResponse<FinanceiroResumoInfo>.Success(FinanceiroMapper.MapResumo(resumo));
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

    public async Task<ApiResponse<IReadOnlyList<LancamentoInfo>>> GetLancamentosAsync(
        LancamentoTipo? tipo = null,
        LancamentoStatus? status = null,
        Guid? contaId = null,
        Guid? categoriaFinanceiraId = null,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        CancellationToken ct = default)
    {
        try
        {
            var token = await authLocal.GetAccessTokenAsync(ct);
            var lancamentos = await api.ObterLancamentosAsync(tipo, status, contaId, categoriaFinanceiraId, dataInicio, dataFim, $"Bearer {token}", ct);
            return ApiResponse<IReadOnlyList<LancamentoInfo>>.Success(lancamentos.Select(FinanceiroMapper.MapLancamento).ToList());
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

    public async Task<ApiResponse<LancamentoInfo>> GetLancamentoAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var token = await authLocal.GetAccessTokenAsync(ct);
            var resp = await api.ObterLancamentoAsync(id, $"Bearer {token}", ct);
            return ApiResponse<LancamentoInfo>.Success(FinanceiroMapper.MapLancamento(resp));
        }
        catch (ApiException apiEx) { return ApiResponse<LancamentoInfo>.Error("Erro ao obter lançamento", GetProblemDetails(apiEx)); }
        catch (Exception ex) { logger.LogError(ex, "Erro"); return ApiResponse<LancamentoInfo>.Error("Erro inesperado."); }
    }

    public async Task<ApiResponse<LancamentoInfo>> CriarLancamentoAsync(LancamentoRequest request, CancellationToken ct = default)
    {
        try
        {
            var token = await authLocal.GetAccessTokenAsync(ct);
            var resp = await api.CriarLancamentoAsync(request, $"Bearer {token}", ct);
            return ApiResponse<LancamentoInfo>.Success(FinanceiroMapper.MapLancamento(resp));
        }
        catch (ApiException apiEx) { return ApiResponse<LancamentoInfo>.Error("Erro ao criar lançamento", GetProblemDetails(apiEx)); }
        catch (Exception ex) { logger.LogError(ex, "Erro"); return ApiResponse<LancamentoInfo>.Error("Erro inesperado."); }
    }

    public async Task<ApiResponse<LancamentoInfo>> AtualizarLancamentoAsync(Guid id, LancamentoRequest request, CancellationToken ct = default)
    {
        try
        {
            var token = await authLocal.GetAccessTokenAsync(ct);
            var resp = await api.AtualizarLancamentoAsync(id, request, $"Bearer {token}", ct);
            return ApiResponse<LancamentoInfo>.Success(FinanceiroMapper.MapLancamento(resp));
        }
        catch (ApiException apiEx) { return ApiResponse<LancamentoInfo>.Error("Erro ao atualizar lançamento", GetProblemDetails(apiEx)); }
        catch (Exception ex) { logger.LogError(ex, "Erro"); return ApiResponse<LancamentoInfo>.Error("Erro inesperado."); }
    }

    public async Task<ApiResponse<bool>> RemoverLancamentoAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var token = await authLocal.GetAccessTokenAsync(ct);
            await api.RemoverAsync(id, $"Bearer {token}", ct);
            return ApiResponse<bool>.Success(true);
        }
        catch (ApiException apiEx) { return ApiResponse<bool>.Error("Erro ao remover lançamento", GetProblemDetails(apiEx)); }
        catch (Exception ex) { logger.LogError(ex, "Erro"); return ApiResponse<bool>.Error("Erro inesperado."); }
    }
}

internal static class FinanceiroMapper
{
    public static FinanceiroResumoInfo MapResumo(FinanceiroResumoResponse r) => new()
    {
        TotalAReceber = r.TotalAReceber,
        TotalAPagar = r.TotalAPagar,
        SaldoPrevisto = r.SaldoPrevisto,
        RecebidoNoMes = r.RecebidoNoMes,
        PagoNoMes = r.PagoNoMes,
        PeriodoLabel = r.PeriodoLabel
    };

    public static LancamentoInfo MapLancamento(LancamentoResponse l) => new()
    {
        Id = l.Id,
        Descricao = l.Descricao,
        Valor = l.Valor,
        DataVencimento = l.DataVencimento,
        DataPagamento = l.DataPagamento,
        Tipo = l.Tipo,
        Status = l.Status,
        FornecedorNome = l.FornecedorNome,
        PedidoId = l.PedidoId,
        ContaId = l.ContaId,
        ContaNome = l.ContaNome,
        CategoriaFinanceiraId = l.CategoriaFinanceiraId,
        CategoriaFinanceiraNome = l.CategoriaFinanceiraNome,
        CategoriaFinanceiraIcone = l.CategoriaFinanceiraIcone,
        CategoriaFinanceiraCor = l.CategoriaFinanceiraCor,
        SubcategoriaFinanceiraId = l.SubcategoriaFinanceiraId,
        SubcategoriaFinanceiraNome = l.SubcategoriaFinanceiraNome,
        Operacao = l.Operacao,
        ParcelaAtual = l.ParcelaAtual,
        ParcelaTotal = l.ParcelaTotal,
        FaturaId = l.FaturaId,
        RecorrenciaId = l.RecorrenciaId,
        Realizado = l.Realizado,
        ValorBaixado = l.ValorBaixado,
        ValorEmAberto = l.ValorEmAberto
    };

    public static ContaInfo MapConta(ContaResponse c) => new()
    {
        Id = c.Id,
        Nome = c.Nome,
        Tipo = c.Tipo,
        Cor = c.Cor,
        Ordem = c.Ordem,
        Limite = c.Limite,
        DiaFechamento = c.DiaFechamento,
        DiaVencimento = c.DiaVencimento,
        SaldoInicial = c.SaldoInicial,
        Ativo = c.Ativo
    };

    public static CategoriaFinanceiraInfo MapCategoria(CategoriaFinanceiraResponse c) => new()
    {
        Id = c.Id,
        Tipo = c.Tipo,
        Nome = c.Nome,
        IconeNome = c.IconeNome,
        Cor = c.Cor,
        Ordem = c.Ordem,
        Ativo = c.Ativo,
        Subcategorias = c.Subcategorias.Select(MapSub).ToList()
    };

    public static SubcategoriaFinanceiraInfo MapSub(SubcategoriaFinanceiraResponse s) => new()
    {
        Id = s.Id,
        CategoriaFinanceiraId = s.CategoriaFinanceiraId,
        Nome = s.Nome,
        IconeNome = s.IconeNome,
        Cor = s.Cor,
        Ordem = s.Ordem,
        Ativo = s.Ativo
    };

    public static FaturaInfo MapFatura(FaturaResponse f) => new()
    {
        Id = f.Id,
        ContaId = f.ContaId,
        ContaNome = f.ContaNome,
        Mes = f.Mes,
        Ano = f.Ano,
        DataInicio = f.DataInicio,
        DataFim = f.DataFim,
        DataVencimento = f.DataVencimento,
        ValorTotal = f.ValorTotal,
        ValorPago = f.ValorPago,
        ValorEmAberto = f.ValorEmAberto,
        Quitada = f.Quitada,
        Lancamentos = f.Lancamentos.Select(MapLancamento).ToList()
    };

    public static RecorrenciaInfo MapRecorrencia(RecorrenciaResponse r) => new()
    {
        Id = r.Id,
        Tipo = r.Tipo,
        Descricao = r.Descricao,
        ContaId = r.ContaId,
        ContaNome = r.ContaNome,
        CategoriaFinanceiraId = r.CategoriaFinanceiraId,
        CategoriaFinanceiraNome = r.CategoriaFinanceiraNome,
        SubcategoriaFinanceiraId = r.SubcategoriaFinanceiraId,
        SubcategoriaFinanceiraNome = r.SubcategoriaFinanceiraNome,
        ValorPadrao = r.ValorPadrao,
        Periodicidade = r.Periodicidade,
        DiaDoMes = r.DiaDoMes,
        DiaDaSemana = r.DiaDaSemana,
        DataInicio = r.DataInicio,
        DataFim = r.DataFim,
        ProximaData = r.ProximaData,
        Ativo = r.Ativo
    };

    public static LancamentoBaixaInfo MapBaixa(LancamentoBaixaResponse b) => new()
    {
        Id = b.Id,
        LancamentoId = b.LancamentoId,
        Data = b.Data,
        Valor = b.Valor,
        ContaId = b.ContaId,
        ContaNome = b.ContaNome,
        ComprovanteFinanceiroId = b.ComprovanteFinanceiroId,
        Observacoes = b.Observacoes
    };

    public static ComprovanteFinanceiroInfo MapComprovante(ComprovanteFinanceiroResponse c) => new()
    {
        Id = c.Id,
        Descricao = c.Descricao,
        FileName = c.FileName,
        ContentType = c.ContentType,
        Size = c.Size,
        Url = c.Url
    };

    public static ExtratoInfo MapExtrato(ExtratoResponse r) => new()
    {
        PeriodoInicio = r.PeriodoInicio,
        PeriodoFim = r.PeriodoFim,
        ContaIds = r.ContaIds.ToList(),
        SaldoInicial = r.SaldoInicial,
        SaldoFinal = r.SaldoFinal,
        TotalEntradas = r.TotalEntradas,
        TotalSaidas = r.TotalSaidas,
        Movimentos = r.Movimentos.Select(MapMovimento).ToList(),
        SaldosDiarios = r.SaldosDiarios.Select(s => new ExtratoSaldoDiarioInfo
        {
            Data = s.Data,
            Entradas = s.Entradas,
            Saidas = s.Saidas,
            SaldoFinalDia = s.SaldoFinalDia
        }).ToList()
    };

    private static ExtratoMovimentoInfo MapMovimento(ExtratoMovimentoResponse m) => new()
    {
        Id = m.Id,
        Origem = m.Origem,
        LancamentoId = m.LancamentoId,
        ContaId = m.ContaId,
        ContaNome = m.ContaNome,
        Data = m.Data,
        Descricao = m.Descricao,
        Tipo = m.Tipo,
        Valor = m.Valor,
        SaldoApos = m.SaldoApos,
        CategoriaNome = m.CategoriaNome,
        CategoriaIcone = m.CategoriaIcone,
        CategoriaCor = m.CategoriaCor,
        Observacoes = m.Observacoes
    };

    public static RelatorioFinanceiroInfo MapRelatorio(RelatorioFinanceiroResponse r) => new()
    {
        Ano = r.Ano,
        Mes = r.Mes,
        Quantidade = r.Quantidade,
        Meses = r.Meses.Select(m => new RelatorioMesInfo
        {
            Ano = m.Ano,
            Mes = m.Mes,
            Label = m.Label,
            Receitas = m.Receitas,
            Despesas = m.Despesas,
            Saldo = m.Saldo
        }).ToList(),
        Receitas = r.Receitas.Select(c => new RelatorioCategoriaInfo
        {
            CategoriaFinanceiraId = c.CategoriaFinanceiraId,
            Nome = c.Nome,
            Icone = c.Icone,
            Cor = c.Cor,
            Tipo = c.Tipo,
            Total = c.Total
        }).ToList(),
        Despesas = r.Despesas.Select(c => new RelatorioCategoriaInfo
        {
            CategoriaFinanceiraId = c.CategoriaFinanceiraId,
            Nome = c.Nome,
            Icone = c.Icone,
            Cor = c.Cor,
            Tipo = c.Tipo,
            Total = c.Total
        }).ToList(),
        TotalReceitas = r.TotalReceitas,
        TotalDespesas = r.TotalDespesas,
        Saldo = r.Saldo
    };
}
