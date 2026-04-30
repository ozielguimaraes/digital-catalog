using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Infrastructure.Data;
using MeuCatalogo.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeuCatalogo.Application.Services;

public sealed class ExtratoService : IExtratoService
{
    private readonly ApplicationDbContext _db;

    public ExtratoService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ApiResponse<ExtratoResponse>> ObterPorContaAsync(Guid contaId, DateTime dataInicio, DateTime dataFim, string userId)
    {
        var conta = await _db.Contas.FirstOrDefaultAsync(c => c.Id == contaId && c.UserId == userId);
        if (conta == null) return ApiResponse<ExtratoResponse>.Error(ResponseType.NotFound, "Conta não encontrada");
        if (conta.EhCartaoCredito())
            return ApiResponse<ExtratoResponse>.Error("Cartões de crédito não têm extrato. Use o endpoint de fatura.");

        return await MontarExtratoAsync(new[] { conta }, dataInicio, dataFim);
    }

    public async Task<ApiResponse<ExtratoResponse>> ObterConsolidadoAsync(DateTime dataInicio, DateTime dataFim, IEnumerable<Guid>? contaIds, string userId)
    {
        var query = _db.Contas
            .Where(c => c.UserId == userId && c.Tipo != ContaTipo.CartaoCredito);

        if (contaIds is not null)
        {
            var ids = contaIds.ToList();
            if (ids.Count > 0) query = query.Where(c => ids.Contains(c.Id));
        }

        var contas = await query.ToListAsync();
        if (contas.Count == 0)
            return ApiResponse<ExtratoResponse>.Error(ResponseType.NotFound, "Nenhuma conta encontrada");

        return await MontarExtratoAsync(contas, dataInicio, dataFim);
    }

    private async Task<ApiResponse<ExtratoResponse>> MontarExtratoAsync(IReadOnlyList<Conta> contas, DateTime dataInicio, DateTime dataFim)
    {
        if (dataFim < dataInicio)
            return ApiResponse<ExtratoResponse>.Error("Data fim deve ser maior ou igual à data início");

        var inicio = new DateTime(dataInicio.Year, dataInicio.Month, dataInicio.Day, 0, 0, 0, DateTimeKind.Utc);
        var fim = new DateTime(dataFim.Year, dataFim.Month, dataFim.Day, 23, 59, 59, DateTimeKind.Utc);
        var contaIds = contas.Select(c => c.Id).ToList();

        var movimentos = await CarregarMovimentosAsync(contaIds, fim);

        var saldoInicial = contas.Sum(c => c.SaldoInicial)
                           + movimentos.Where(m => m.Data < inicio).Sum(m => Sinal(m) * m.Valor);

        var dentroPeriodo = movimentos
            .Where(m => m.Data >= inicio && m.Data <= fim)
            .OrderBy(m => m.Data).ThenBy(m => m.LancamentoId).ThenBy(m => m.Id)
            .ToList();

        decimal saldo = saldoInicial;
        decimal totalEntradas = 0m;
        decimal totalSaidas = 0m;
        var lista = new List<ExtratoMovimentoResponse>(dentroPeriodo.Count);

        foreach (var m in dentroPeriodo)
        {
            var sinal = Sinal(m);
            var valorComSinal = sinal * m.Valor;
            saldo += valorComSinal;

            if (sinal > 0) totalEntradas += m.Valor;
            else totalSaidas += m.Valor;

            lista.Add(new ExtratoMovimentoResponse
            {
                Id = m.Id,
                Origem = m.Origem,
                LancamentoId = m.LancamentoId,
                ContaId = m.ContaId,
                ContaNome = contas.First(c => c.Id == m.ContaId).Nome,
                Data = m.Data,
                Descricao = m.Descricao,
                Tipo = sinal > 0 ? ExtratoMovimentoTipo.Entrada : ExtratoMovimentoTipo.Saida,
                Valor = m.Valor,
                SaldoApos = saldo,
                CategoriaNome = m.CategoriaNome,
                CategoriaIcone = m.CategoriaIcone,
                CategoriaCor = m.CategoriaCor,
                Observacoes = m.Observacoes
            });
        }

        var saldosDiarios = MontarSaldosDiarios(lista, saldoInicial);

        var resp = new ExtratoResponse
        {
            PeriodoInicio = inicio,
            PeriodoFim = fim,
            ContaIds = contaIds,
            SaldoInicial = saldoInicial,
            SaldoFinal = saldo,
            TotalEntradas = totalEntradas,
            TotalSaidas = totalSaidas,
            Movimentos = lista,
            SaldosDiarios = saldosDiarios
        };
        return ApiResponse<ExtratoResponse>.Success(resp);
    }

    private async Task<List<MovimentoBruto>> CarregarMovimentosAsync(List<Guid> contaIds, DateTime ateData)
    {
        var baixas = await _db.LancamentosBaixas
            .Include(b => b.Lancamento!).ThenInclude(l => l!.CategoriaFinanceira)
            .Where(b => b.Ativo
                        && contaIds.Contains(b.ContaId)
                        && b.Data <= ateData
                        && b.Lancamento!.Ativo)
            .ToListAsync();

        var lancamentosRealizados = await _db.Lancamentos
            .Include(l => l.CategoriaFinanceira)
            .Include(l => l.Baixas.Where(b => b.Ativo))
            .Where(l => l.Ativo
                        && l.Realizado
                        && l.ContaId.HasValue
                        && contaIds.Contains(l.ContaId.Value)
                        && l.DataVencimento <= ateData
                        && !l.Baixas.Any(b => b.Ativo))
            .ToListAsync();

        var lista = new List<MovimentoBruto>(baixas.Count + lancamentosRealizados.Count);

        foreach (var b in baixas)
        {
            lista.Add(new MovimentoBruto
            {
                Id = b.Id,
                Origem = ExtratoMovimentoOrigem.Baixa,
                LancamentoId = b.LancamentoId,
                ContaId = b.ContaId,
                Data = b.Data,
                Descricao = b.Lancamento!.Descricao,
                Valor = b.Valor,
                LancamentoTipo = b.Lancamento.Tipo,
                CategoriaNome = b.Lancamento.CategoriaFinanceira?.Nome,
                CategoriaIcone = b.Lancamento.CategoriaFinanceira?.IconeNome,
                CategoriaCor = b.Lancamento.CategoriaFinanceira?.Cor,
                Observacoes = b.Observacoes
            });
        }

        foreach (var l in lancamentosRealizados)
        {
            lista.Add(new MovimentoBruto
            {
                Id = l.Id,
                Origem = ExtratoMovimentoOrigem.Lancamento,
                LancamentoId = l.Id,
                ContaId = l.ContaId!.Value,
                Data = l.DataPagamento ?? l.DataVencimento,
                Descricao = l.Descricao,
                Valor = l.Valor,
                LancamentoTipo = l.Tipo,
                CategoriaNome = l.CategoriaFinanceira?.Nome,
                CategoriaIcone = l.CategoriaFinanceira?.IconeNome,
                CategoriaCor = l.CategoriaFinanceira?.Cor,
                Observacoes = l.Observacoes
            });
        }

        return lista;
    }

    private static int Sinal(MovimentoBruto m) => m.LancamentoTipo == LancamentoTipo.Receber ? 1 : -1;

    private static List<ExtratoSaldoDiarioResponse> MontarSaldosDiarios(List<ExtratoMovimentoResponse> movs, decimal saldoInicial)
    {
        if (movs.Count == 0) return new();
        var grupos = movs.GroupBy(m => m.Data.Date).OrderBy(g => g.Key);
        var resultado = new List<ExtratoSaldoDiarioResponse>();
        decimal saldoAcumulado = saldoInicial;
        foreach (var g in grupos)
        {
            var entradas = g.Where(m => m.Tipo == ExtratoMovimentoTipo.Entrada).Sum(m => m.Valor);
            var saidas = g.Where(m => m.Tipo == ExtratoMovimentoTipo.Saida).Sum(m => m.Valor);
            saldoAcumulado = g.Last().SaldoApos;
            resultado.Add(new ExtratoSaldoDiarioResponse
            {
                Data = g.Key,
                Entradas = entradas,
                Saidas = saidas,
                SaldoFinalDia = saldoAcumulado
            });
        }
        return resultado;
    }

    private sealed class MovimentoBruto
    {
        public Guid Id { get; set; }
        public ExtratoMovimentoOrigem Origem { get; set; }
        public Guid LancamentoId { get; set; }
        public Guid ContaId { get; set; }
        public DateTime Data { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public LancamentoTipo LancamentoTipo { get; set; }
        public string? CategoriaNome { get; set; }
        public string? CategoriaIcone { get; set; }
        public string? CategoriaCor { get; set; }
        public string? Observacoes { get; set; }
    }
}
