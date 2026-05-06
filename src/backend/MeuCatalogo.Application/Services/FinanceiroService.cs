using System.Globalization;
using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Infrastructure.Data;
using MeuCatalogo.Application.Interfaces;
using MeuCatalogo.Application.Services.Faturas;
using Microsoft.EntityFrameworkCore;

namespace MeuCatalogo.Application.Services;

public sealed class FinanceiroService : IFinanceiroService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IRecorrenciaService? _recorrenciaService;
    private readonly IFaturaService? _faturaService;

    public FinanceiroService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public FinanceiroService(ApplicationDbContext dbContext, IRecorrenciaService recorrenciaService, IFaturaService faturaService)
    {
        _dbContext = dbContext;
        _recorrenciaService = recorrenciaService;
        _faturaService = faturaService;
    }

    public async Task<ApiResponse<List<LancamentoResponse>>> GetAllAsync(string userId, LancamentoTipo? tipo = null)
    {
        return await ListarAsync(userId, new LancamentoFiltro { Tipo = tipo, IncluirRecorrenciasFuturas = false });
    }

    public async Task<ApiResponse<List<LancamentoResponse>>> ListarAsync(string userId, LancamentoFiltro filtro)
    {
        try
        {
            if (filtro.IncluirRecorrenciasFuturas && _recorrenciaService != null)
            {
                var ate = filtro.DataFim ?? DateTime.UtcNow.AddDays(60);
                await _recorrenciaService.GerarPendentesAsync(userId, ate);
            }

            var query = _dbContext.Lancamentos
                .Include(l => l.Fornecedor)
                .Include(l => l.Conta)
                .Include(l => l.CategoriaFinanceira)
                .Include(l => l.SubcategoriaFinanceira)
                .Include(l => l.Baixas.Where(b => b.Ativo))
                .Where(l => l.UserId == userId && l.Ativo);

            if (filtro.Tipo.HasValue) query = query.Where(l => l.Tipo == filtro.Tipo.Value);
            if (filtro.Status.HasValue) query = query.Where(l => l.Status == filtro.Status.Value);
            if (filtro.ContaId.HasValue) query = query.Where(l => l.ContaId == filtro.ContaId.Value);
            if (filtro.CategoriaFinanceiraId.HasValue) query = query.Where(l => l.CategoriaFinanceiraId == filtro.CategoriaFinanceiraId.Value);
            if (filtro.DataInicio.HasValue) query = query.Where(l => l.DataVencimento >= filtro.DataInicio.Value);
            if (filtro.DataFim.HasValue) query = query.Where(l => l.DataVencimento <= filtro.DataFim.Value);

            var lancamentos = await query.OrderBy(l => l.DataVencimento).ToListAsync();
            return ApiResponse<List<LancamentoResponse>>.Success(lancamentos.Select(LancamentoMapper.MapToResponse).ToList());
        }
        catch (Exception ex)
        {
            return ApiResponse<List<LancamentoResponse>>.Error($"Erro ao buscar lançamentos: {ex.Message}");
        }
    }

    public async Task<ApiResponse<LancamentoResponse>> GetByIdAsync(Guid id, string userId)
    {
        try
        {
            var lancamento = await _dbContext.Lancamentos
                .Include(l => l.Fornecedor)
                .Include(l => l.Conta)
                .Include(l => l.CategoriaFinanceira)
                .Include(l => l.SubcategoriaFinanceira)
                .Include(l => l.Baixas.Where(b => b.Ativo))
                .FirstOrDefaultAsync(l => l.Id == id && l.UserId == userId);

            if (lancamento == null)
                return ApiResponse<LancamentoResponse>.Error("Lançamento não encontrado");

            return ApiResponse<LancamentoResponse>.Success(LancamentoMapper.MapToResponse(lancamento));
        }
        catch (Exception ex)
        {
            return ApiResponse<LancamentoResponse>.Error($"Erro ao buscar lançamento: {ex.Message}");
        }
    }

    public async Task<ApiResponse<LancamentoResponse>> CreateAsync(LancamentoRequest request, string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Descricao))
                return ApiResponse<LancamentoResponse>.Error("Descrição é obrigatória");
            if (request.Valor <= 0)
                return ApiResponse<LancamentoResponse>.Error("Valor deve ser maior que zero");

            Conta? conta = null;
            if (request.ContaId.HasValue)
            {
                conta = await _dbContext.Contas.FirstOrDefaultAsync(c => c.Id == request.ContaId.Value && c.UserId == userId);
                if (conta == null) return ApiResponse<LancamentoResponse>.Error("Conta não encontrada");
            }

            var totalParcelas = request.ParcelaTotal.GetValueOrDefault(1);
            if (totalParcelas < 1) totalParcelas = 1;
            if (totalParcelas > 1 && conta == null)
                return ApiResponse<LancamentoResponse>.Error("Conta é obrigatória para parcelamento");

            var lancamentoBase = await ConstruirLancamento(request, userId, conta, parcelaAtual: 1, totalParcelas: totalParcelas, dataVenc: request.DataVencimento);
            _dbContext.Lancamentos.Add(lancamentoBase);

            if (totalParcelas > 1)
            {
                for (short n = 2; n <= totalParcelas; n++)
                {
                    var dataParcela = request.DataVencimento.AddMonths(n - 1);
                    var parcela = await ConstruirLancamento(request, userId, conta, parcelaAtual: n, totalParcelas: totalParcelas, dataVenc: dataParcela);
                    parcela.Descricao = $"{request.Descricao} ({n}/{totalParcelas})";
                    _dbContext.Lancamentos.Add(parcela);
                }
                lancamentoBase.Descricao = $"{request.Descricao} (1/{totalParcelas})";
            }

            await _dbContext.SaveChangesAsync();

            var created = await _dbContext.Lancamentos
                .Include(l => l.Fornecedor)
                .Include(l => l.Conta)
                .Include(l => l.CategoriaFinanceira)
                .Include(l => l.SubcategoriaFinanceira)
                .FirstAsync(l => l.Id == lancamentoBase.Id);

            return ApiResponse<LancamentoResponse>.Success(ResponseType.Created, LancamentoMapper.MapToResponse(created));
        }
        catch (Exception ex)
        {
            return ApiResponse<LancamentoResponse>.Error($"Erro ao criar lançamento: {ex.Message}");
        }
    }

    public async Task<ApiResponse<LancamentoResponse>> UpdateAsync(Guid id, LancamentoRequest request, string userId)
    {
        try
        {
            var lancamento = await _dbContext.Lancamentos
                .FirstOrDefaultAsync(l => l.Id == id && l.UserId == userId);

            if (lancamento == null)
                return ApiResponse<LancamentoResponse>.Error("Lançamento não encontrado");

            lancamento.Descricao = request.Descricao;
            lancamento.Valor = request.Valor;
            lancamento.DataVencimento = request.DataVencimento;
            lancamento.DataPagamento = request.DataPagamento;
            lancamento.Tipo = request.Tipo;
            lancamento.Status = request.Status;
            lancamento.Observacoes = request.Observacoes;
            lancamento.PedidoId = request.PedidoId;
            lancamento.FornecedorId = request.FornecedorId;
            lancamento.ContaId = request.ContaId;
            lancamento.CategoriaFinanceiraId = request.CategoriaFinanceiraId;
            lancamento.SubcategoriaFinanceiraId = request.SubcategoriaFinanceiraId;
            lancamento.ComprovanteFinanceiroId = request.ComprovanteFinanceiroId;
            lancamento.Realizado = request.Realizado;
            lancamento.DataAtualizacao = DateTime.UtcNow;

            _dbContext.Lancamentos.Update(lancamento);
            await _dbContext.SaveChangesAsync();

            var updated = await _dbContext.Lancamentos
                .Include(l => l.Fornecedor)
                .Include(l => l.Conta)
                .Include(l => l.CategoriaFinanceira)
                .Include(l => l.SubcategoriaFinanceira)
                .Include(l => l.Baixas.Where(b => b.Ativo))
                .FirstAsync(l => l.Id == id);

            return ApiResponse<LancamentoResponse>.Success(LancamentoMapper.MapToResponse(updated));
        }
        catch (Exception ex)
        {
            return ApiResponse<LancamentoResponse>.Error($"Erro ao atualizar lançamento: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, string userId)
    {
        try
        {
            var lancamento = await _dbContext.Lancamentos
                .FirstOrDefaultAsync(l => l.Id == id && l.UserId == userId);

            if (lancamento == null)
                return ApiResponse<bool>.Error("Lançamento não encontrado");

            _dbContext.Lancamentos.Remove(lancamento);
            await _dbContext.SaveChangesAsync();

            return ApiResponse<bool>.Success(true, "Lançamento removido com sucesso");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Error($"Erro ao remover lançamento: {ex.Message}");
        }
    }

    public async Task<ApiResponse<FinanceiroResumoResponse>> GetResumoAsync(string userId)
    {
        try
        {
            var hoje = DateTime.UtcNow;
            var inicioMes = new DateTime(hoje.Year, hoje.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var fimMes = inicioMes.AddMonths(1);

            var lancamentos = await _dbContext.Lancamentos
                .Where(l => l.UserId == userId && l.Ativo)
                .ToListAsync();

            var aReceber = lancamentos
                .Where(l => l.Tipo == LancamentoTipo.Receber && l.Status != LancamentoStatus.Pago && l.Status != LancamentoStatus.Cancelado)
                .Sum(l => l.Valor);

            var aPagar = lancamentos
                .Where(l => l.Tipo == LancamentoTipo.Pagar && l.Status != LancamentoStatus.Pago && l.Status != LancamentoStatus.Cancelado)
                .Sum(l => l.Valor);

            var recebidoMes = lancamentos
                .Where(l => l.Tipo == LancamentoTipo.Receber && l.Status == LancamentoStatus.Pago && l.DataPagamento >= inicioMes && l.DataPagamento < fimMes)
                .Sum(l => l.Valor);

            var pagoMes = lancamentos
                .Where(l => l.Tipo == LancamentoTipo.Pagar && l.Status == LancamentoStatus.Pago && l.DataPagamento >= inicioMes && l.DataPagamento < fimMes)
                .Sum(l => l.Valor);

            var resumo = new FinanceiroResumoResponse
            {
                TotalAReceber = aReceber,
                TotalAPagar = aPagar,
                SaldoPrevisto = aReceber - aPagar,
                RecebidoNoMes = recebidoMes,
                PagoNoMes = pagoMes,
                PeriodoLabel = $"{CultureInfo.GetCultureInfo("pt-BR").DateTimeFormat.GetMonthName(hoje.Month).ToLowerInvariant()} · {hoje.Year}"
            };

            return ApiResponse<FinanceiroResumoResponse>.Success(resumo);
        }
        catch (Exception ex)
        {
            return ApiResponse<FinanceiroResumoResponse>.Error($"Erro ao calcular resumo: {ex.Message}");
        }
    }

    private async Task<Lancamento> ConstruirLancamento(LancamentoRequest request, string userId, Conta? conta, short parcelaAtual, short totalParcelas, DateTime dataVenc)
    {
        Guid? faturaId = null;
        if (conta != null && conta.EhCartaoCredito() && _faturaService != null
            && conta.DiaFechamento.HasValue && conta.DiaVencimento.HasValue)
        {
            var (mes, ano) = FaturaCalculator.FaturaParaData(conta.DiaFechamento.Value, dataVenc);
            var fatura = await _faturaService.ObterOuCriarAsync(conta, mes, ano);
            faturaId = fatura.Id;
            dataVenc = fatura.DataVencimento;
        }

        return new Lancamento
        {
            Descricao = request.Descricao,
            Valor = request.Valor,
            DataVencimento = dataVenc,
            DataPagamento = request.DataPagamento,
            Tipo = request.Tipo,
            Status = request.Status,
            Observacoes = request.Observacoes,
            PedidoId = request.PedidoId,
            FornecedorId = request.FornecedorId,
            UserId = userId,
            ContaId = request.ContaId,
            CategoriaFinanceiraId = request.CategoriaFinanceiraId,
            SubcategoriaFinanceiraId = request.SubcategoriaFinanceiraId,
            ComprovanteFinanceiroId = request.ComprovanteFinanceiroId,
            ParcelaAtual = totalParcelas > 1 ? parcelaAtual : (short?)null,
            ParcelaTotal = totalParcelas > 1 ? totalParcelas : (short?)null,
            FaturaId = faturaId,
            Realizado = request.Realizado
        };
    }
}
