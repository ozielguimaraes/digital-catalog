using System.Globalization;
using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Infrastructure.Data;
using MeuCatalogo.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeuCatalogo.Application.Services;

public sealed class RelatorioFinanceiroService : IRelatorioFinanceiroService
{
    private readonly ApplicationDbContext _db;

    public RelatorioFinanceiroService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ApiResponse<RelatorioFinanceiroResponse>> LancamentosPorCategoriaAsync(RelatorioFinanceiroRequest request, string userId)
    {
        if (request.Mes is < 1 or > 12) return ApiResponse<RelatorioFinanceiroResponse>.Error("Mês inválido");
        if (request.Quantidade < 1 || request.Quantidade > 24) return ApiResponse<RelatorioFinanceiroResponse>.Error("Quantidade deve estar entre 1 e 24");

        var inicio = new DateTime(request.Ano, request.Mes, 1, 0, 0, 0, DateTimeKind.Utc);
        var fim = inicio.AddMonths(request.Quantidade).AddDays(-1).AddSeconds(86399);

        var query = _db.Lancamentos
            .Include(l => l.CategoriaFinanceira)
            .Include(l => l.Conta)
            .Include(l => l.Fatura)
            .Include(l => l.Baixas.Where(b => b.Ativo))
            .Where(l => l.UserId == userId && l.Ativo
                        && l.Operacao == LancamentoOperacao.Simples);

        if (request.ContaIds is { Count: > 0 })
            query = query.Where(l => l.ContaId.HasValue && request.ContaIds.Contains(l.ContaId.Value));
        if (request.CategoriaFinanceiraIds is { Count: > 0 })
            query = query.Where(l => l.CategoriaFinanceiraId.HasValue && request.CategoriaFinanceiraIds.Contains(l.CategoriaFinanceiraId.Value));
        if (request.Tipo.HasValue)
            query = query.Where(l => request.Tipo == CategoriaFinanceiraTipo.Receita
                ? l.Tipo == LancamentoTipo.Receber
                : l.Tipo == LancamentoTipo.Pagar);
        if (request.ApenasRealizados)
            query = query.Where(l => l.Realizado);

        var lancamentos = await query.ToListAsync();

        var meses = Enumerable.Range(0, request.Quantidade)
            .Select(i => inicio.AddMonths(i))
            .Select(d => new RelatorioMesItem
            {
                Ano = d.Year,
                Mes = d.Month,
                Label = $"{CultureInfo.GetCultureInfo("pt-BR").DateTimeFormat.GetAbbreviatedMonthName(d.Month).TrimEnd('.')}/{d.Year}"
            })
            .ToList();

        var receitas = new List<RelatorioCategoriaItem>();
        var despesas = new List<RelatorioCategoriaItem>();

        var grouped = lancamentos
            .GroupBy(l => new
            {
                CategoriaId = l.CategoriaFinanceiraId,
                CategoriaNome = l.CategoriaFinanceira?.Nome ?? "(Sem categoria)",
                Icone = l.CategoriaFinanceira?.IconeNome,
                Cor = l.CategoriaFinanceira?.Cor,
                TipoLanc = l.Tipo
            });

        foreach (var g in grouped)
        {
            var tipoCat = g.Key.TipoLanc == LancamentoTipo.Receber
                ? CategoriaFinanceiraTipo.Receita
                : CategoriaFinanceiraTipo.Despesa;

            var item = new RelatorioCategoriaItem
            {
                CategoriaFinanceiraId = g.Key.CategoriaId,
                Nome = g.Key.CategoriaNome,
                Icone = g.Key.Icone,
                Cor = g.Key.Cor,
                Tipo = tipoCat,
                Mensal = meses.Select(m => new RelatorioMesItem { Ano = m.Ano, Mes = m.Mes, Label = m.Label }).ToList()
            };

            foreach (var l in g)
            {
                ApplicarValor(l, request.Regime, item.Mensal, inicio, fim);
            }
            item.Total = item.Mensal.Sum(m => tipoCat == CategoriaFinanceiraTipo.Receita ? m.Receitas : m.Despesas);

            if (tipoCat == CategoriaFinanceiraTipo.Receita) receitas.Add(item);
            else despesas.Add(item);
        }

        foreach (var l in lancamentos)
        {
            ApplicarValor(l, request.Regime, meses, inicio, fim);
        }

        foreach (var m in meses) m.Saldo = m.Receitas - m.Despesas;

        var resp = new RelatorioFinanceiroResponse
        {
            Ano = request.Ano,
            Mes = request.Mes,
            Quantidade = request.Quantidade,
            Meses = meses,
            Receitas = receitas.OrderByDescending(r => r.Total).ToList(),
            Despesas = despesas.OrderByDescending(d => d.Total).ToList(),
            TotalReceitas = receitas.Sum(r => r.Total),
            TotalDespesas = despesas.Sum(d => d.Total)
        };
        resp.Saldo = resp.TotalReceitas - resp.TotalDespesas;

        return ApiResponse<RelatorioFinanceiroResponse>.Success(resp);
    }

    private static void ApplicarValor(Lancamento l, RegimeContabil regime, List<RelatorioMesItem> meses, DateTime inicio, DateTime fim)
    {
        if (regime == RegimeContabil.Competencia)
        {
            var data = l.DataVencimento;
            if (data < inicio || data > fim) return;
            var bucket = meses.FirstOrDefault(m => m.Ano == data.Year && m.Mes == data.Month);
            if (bucket == null) return;
            if (l.Tipo == LancamentoTipo.Receber) bucket.Receitas += l.Valor;
            else bucket.Despesas += l.Valor;
            return;
        }

        var ehCartao = l.Conta?.Tipo == ContaTipo.CartaoCredito;
        if (ehCartao && l.Fatura != null)
        {
            var data = l.Fatura.DataVencimento;
            if (data < inicio || data > fim) return;
            var bucket = meses.FirstOrDefault(m => m.Ano == data.Year && m.Mes == data.Month);
            if (bucket == null) return;
            if (l.Tipo == LancamentoTipo.Receber) bucket.Receitas += l.Valor;
            else bucket.Despesas += l.Valor;
            return;
        }

        var baixas = l.Baixas?.Where(b => b.Ativo && b.Data >= inicio && b.Data <= fim).ToList() ?? new List<LancamentoBaixa>();
        if (baixas.Count > 0)
        {
            foreach (var b in baixas)
            {
                var bucket = meses.FirstOrDefault(m => m.Ano == b.Data.Year && m.Mes == b.Data.Month);
                if (bucket == null) continue;
                if (l.Tipo == LancamentoTipo.Receber) bucket.Receitas += b.Valor;
                else bucket.Despesas += b.Valor;
            }
            return;
        }

        if (l.Realizado)
        {
            var data = l.DataPagamento ?? l.DataVencimento;
            if (data < inicio || data > fim) return;
            var bucket = meses.FirstOrDefault(m => m.Ano == data.Year && m.Mes == data.Month);
            if (bucket == null) return;
            if (l.Tipo == LancamentoTipo.Receber) bucket.Receitas += l.Valor;
            else bucket.Despesas += l.Valor;
        }
    }
}
