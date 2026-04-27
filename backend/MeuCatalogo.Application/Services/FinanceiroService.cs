using System.Globalization;
using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Infrastructure.Data;
using MeuCatalogo.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeuCatalogo.Application.Services;

public sealed class FinanceiroService : IFinanceiroService
{
    private readonly ApplicationDbContext _dbContext;

    public FinanceiroService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApiResponse<List<LancamentoResponse>>> GetAllAsync(string userId, LancamentoTipo? tipo = null)
    {
        try
        {
            var query = _dbContext.Lancamentos
                .Include(l => l.Fornecedor)
                .Where(l => l.UserId == userId && l.Ativo);

            if (tipo.HasValue)
                query = query.Where(l => l.Tipo == tipo.Value);

            var lancamentos = await query
                .OrderBy(l => l.DataVencimento)
                .ToListAsync();

            return ApiResponse<List<LancamentoResponse>>.Success(lancamentos.Select(MapToResponse).ToList());
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
                .FirstOrDefaultAsync(l => l.Id == id && l.UserId == userId);

            if (lancamento == null)
                return ApiResponse<LancamentoResponse>.Error("Lançamento não encontrado");

            return ApiResponse<LancamentoResponse>.Success(MapToResponse(lancamento));
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

            var lancamento = new Lancamento
            {
                Descricao = request.Descricao,
                Valor = request.Valor,
                DataVencimento = request.DataVencimento,
                DataPagamento = request.DataPagamento,
                Tipo = request.Tipo,
                Status = request.Status,
                Observacoes = request.Observacoes,
                PedidoId = request.PedidoId,
                FornecedorId = request.FornecedorId,
                UserId = userId
            };

            await _dbContext.Lancamentos.AddAsync(lancamento);
            await _dbContext.SaveChangesAsync();

            var created = await _dbContext.Lancamentos
                .Include(l => l.Fornecedor)
                .FirstAsync(l => l.Id == lancamento.Id);

            return ApiResponse<LancamentoResponse>.Success(MapToResponse(created));
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
            lancamento.DataAtualizacao = DateTime.UtcNow;

            _dbContext.Lancamentos.Update(lancamento);
            await _dbContext.SaveChangesAsync();

            var updated = await _dbContext.Lancamentos
                .Include(l => l.Fornecedor)
                .FirstAsync(l => l.Id == id);

            return ApiResponse<LancamentoResponse>.Success(MapToResponse(updated));
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

    private static LancamentoResponse MapToResponse(Lancamento l) => new()
    {
        Id = l.Id,
        Descricao = l.Descricao,
        Valor = l.Valor,
        DataVencimento = l.DataVencimento,
        DataPagamento = l.DataPagamento,
        Tipo = l.Tipo,
        Status = l.Status,
        Observacoes = l.Observacoes,
        PedidoId = l.PedidoId,
        FornecedorId = l.FornecedorId,
        FornecedorNome = l.Fornecedor?.Nome,
        CreatedAt = l.DataCriacao,
        UpdatedAt = l.DataAtualizacao
    };
}
