using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Infrastructure.Data;
using MeuCatalogo.Application.Interfaces;
using MeuCatalogo.Application.Services.Faturas;
using Microsoft.EntityFrameworkCore;

namespace MeuCatalogo.Application.Services;

public sealed class FaturaService : IFaturaService
{
    private readonly ApplicationDbContext _db;

    public FaturaService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ApiResponse<FaturaResponse>> GetAsync(Guid contaId, int mes, int ano, string userId)
    {
        var conta = await _db.Contas.FirstOrDefaultAsync(c => c.Id == contaId && c.UserId == userId);
        if (conta == null) return ApiResponse<FaturaResponse>.Error(ResponseType.NotFound, "Conta não encontrada");
        if (!conta.EhCartaoCredito()) return ApiResponse<FaturaResponse>.Error("Apenas cartões de crédito têm fatura");

        var fatura = await ObterOuCriarAsync(conta, mes, ano);
        var lancamentos = await _db.Lancamentos
            .Include(l => l.CategoriaFinanceira)
            .Include(l => l.SubcategoriaFinanceira)
            .Where(l => l.FaturaId == fatura.Id && l.Ativo)
            .OrderBy(l => l.DataVencimento)
            .ToListAsync();

        return ApiResponse<FaturaResponse>.Success(MapToResponse(fatura, conta, lancamentos));
    }

    public async Task<ApiResponse<List<FaturaResponse>>> ListarPorContaAsync(Guid contaId, string userId, int? ano = null)
    {
        var conta = await _db.Contas.FirstOrDefaultAsync(c => c.Id == contaId && c.UserId == userId);
        if (conta == null) return ApiResponse<List<FaturaResponse>>.Error(ResponseType.NotFound, "Conta não encontrada");

        var q = _db.Faturas.Where(f => f.ContaId == contaId && f.Ativo);
        if (ano.HasValue) q = q.Where(f => f.Ano == ano.Value);

        var faturas = await q.OrderByDescending(f => f.Ano).ThenByDescending(f => f.Mes).ToListAsync();
        var ids = faturas.Select(f => f.Id).ToList();
        var lancs = await _db.Lancamentos
            .Where(l => l.FaturaId.HasValue && ids.Contains(l.FaturaId!.Value) && l.Ativo)
            .ToListAsync();

        var resp = faturas
            .Select(f => MapToResponse(f, conta, lancs.Where(l => l.FaturaId == f.Id).ToList()))
            .ToList();
        return ApiResponse<List<FaturaResponse>>.Success(resp);
    }

    public async Task<Fatura> ObterOuCriarAsync(Conta cartao, int mes, int ano)
    {
        if (!cartao.EhCartaoCredito())
            throw new InvalidOperationException("Conta não é cartão de crédito");
        if (!cartao.DiaFechamento.HasValue || !cartao.DiaVencimento.HasValue)
            throw new InvalidOperationException("Cartão sem dias configurados");

        var existente = await _db.Faturas.FirstOrDefaultAsync(f => f.ContaId == cartao.Id && f.Mes == mes && f.Ano == ano);
        if (existente != null) return existente;

        var (inicio, fim, venc) = FaturaCalculator.Calcular(
            cartao.DiaFechamento.Value, cartao.DiaVencimento.Value, mes, ano);

        var nova = new Fatura
        {
            ContaId = cartao.Id,
            Mes = mes,
            Ano = ano,
            DataInicio = inicio,
            DataFim = fim,
            DataVencimento = venc,
            ValorPago = null
        };
        _db.Faturas.Add(nova);
        await _db.SaveChangesAsync();
        return nova;
    }

    public async Task<ApiResponse<bool>> RegistrarPagamentoAsync(Guid faturaId, decimal valor, string userId)
    {
        if (valor <= 0) return ApiResponse<bool>.Error("Valor deve ser maior que zero");

        var fatura = await _db.Faturas.Include(f => f.Conta).FirstOrDefaultAsync(f => f.Id == faturaId);
        if (fatura == null || fatura.Conta?.UserId != userId)
            return ApiResponse<bool>.Error(ResponseType.NotFound, "Fatura não encontrada");

        fatura.ValorPago = (fatura.ValorPago ?? 0m) + valor;
        fatura.DataAtualizacao = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ApiResponse<bool>.Success(true);
    }

    private static FaturaResponse MapToResponse(Fatura f, Conta conta, List<Lancamento> lancs)
    {
        var total = lancs.Sum(l => l.Valor);
        var pago = f.ValorPago ?? 0m;
        return new FaturaResponse
        {
            Id = f.Id,
            ContaId = f.ContaId,
            ContaNome = conta.Nome,
            Mes = f.Mes,
            Ano = f.Ano,
            DataInicio = f.DataInicio,
            DataFim = f.DataFim,
            DataVencimento = f.DataVencimento,
            ValorTotal = total,
            ValorPago = f.ValorPago,
            ValorEmAberto = Math.Max(0m, total - pago),
            Quitada = pago >= total && total > 0m,
            Lancamentos = lancs.Select(LancamentoMapper.MapToResponse).ToList()
        };
    }
}
