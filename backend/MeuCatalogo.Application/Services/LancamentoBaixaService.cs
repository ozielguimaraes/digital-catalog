using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Infrastructure.Data;
using MeuCatalogo.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeuCatalogo.Application.Services;

public sealed class LancamentoBaixaService : ILancamentoBaixaService
{
    private readonly ApplicationDbContext _db;

    public LancamentoBaixaService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ApiResponse<List<LancamentoBaixaResponse>>> ListarAsync(Guid lancamentoId, string userId)
    {
        var lanc = await _db.Lancamentos.FirstOrDefaultAsync(l => l.Id == lancamentoId && l.UserId == userId);
        if (lanc == null) return ApiResponse<List<LancamentoBaixaResponse>>.Error(ResponseType.NotFound, "Lançamento não encontrado");

        var baixas = await _db.LancamentosBaixas
            .Include(b => b.Conta)
            .Where(b => b.LancamentoId == lancamentoId && b.Ativo)
            .OrderBy(b => b.Data)
            .ToListAsync();
        return ApiResponse<List<LancamentoBaixaResponse>>.Success(baixas.Select(MapToResponse).ToList());
    }

    public async Task<ApiResponse<LancamentoBaixaResponse>> AdicionarAsync(Guid lancamentoId, LancamentoBaixaRequest request, string userId)
    {
        if (request.Valor <= 0) return ApiResponse<LancamentoBaixaResponse>.Error("Valor deve ser maior que zero");
        if (request.Data == default) return ApiResponse<LancamentoBaixaResponse>.Error("Data é obrigatória");

        var lanc = await _db.Lancamentos
            .Include(l => l.Baixas)
            .FirstOrDefaultAsync(l => l.Id == lancamentoId && l.UserId == userId);
        if (lanc == null) return ApiResponse<LancamentoBaixaResponse>.Error(ResponseType.NotFound, "Lançamento não encontrado");

        var conta = await _db.Contas.AnyAsync(c => c.Id == request.ContaId && c.UserId == userId);
        if (!conta) return ApiResponse<LancamentoBaixaResponse>.Error("Conta não encontrada");

        var jaBaixado = lanc.Baixas.Where(b => b.Ativo).Sum(b => b.Valor);
        var disponivel = lanc.Valor - jaBaixado;
        if (request.Valor > disponivel)
            return ApiResponse<LancamentoBaixaResponse>.Error($"Valor excede o saldo em aberto ({disponivel:0.00})");

        var baixa = new LancamentoBaixa
        {
            LancamentoId = lancamentoId,
            Data = request.Data,
            Valor = request.Valor,
            ContaId = request.ContaId,
            ComprovanteFinanceiroId = request.ComprovanteFinanceiroId,
            Observacoes = request.Observacoes
        };
        _db.LancamentosBaixas.Add(baixa);

        var totalBaixado = jaBaixado + request.Valor;
        AtualizarStatus(lanc, totalBaixado, request.Data);

        await _db.SaveChangesAsync();
        var carregada = await _db.LancamentosBaixas.Include(b => b.Conta).FirstAsync(b => b.Id == baixa.Id);
        return ApiResponse<LancamentoBaixaResponse>.Success(ResponseType.Created, MapToResponse(carregada), "Baixa registrada");
    }

    public async Task<ApiResponse<bool>> RemoverAsync(Guid lancamentoId, Guid baixaId, string userId)
    {
        var lanc = await _db.Lancamentos
            .Include(l => l.Baixas)
            .FirstOrDefaultAsync(l => l.Id == lancamentoId && l.UserId == userId);
        if (lanc == null) return ApiResponse<bool>.Error(ResponseType.NotFound, "Lançamento não encontrado");

        var baixa = lanc.Baixas.FirstOrDefault(b => b.Id == baixaId);
        if (baixa == null) return ApiResponse<bool>.Error(ResponseType.NotFound, "Baixa não encontrada");

        _db.LancamentosBaixas.Remove(baixa);

        var novoTotal = lanc.Baixas.Where(b => b.Ativo && b.Id != baixaId).Sum(b => b.Valor);
        AtualizarStatus(lanc, novoTotal, null);
        await _db.SaveChangesAsync();
        return ApiResponse<bool>.Success(true);
    }

    private static void AtualizarStatus(Lancamento lanc, decimal totalBaixado, DateTime? dataUltimaBaixa)
    {
        if (totalBaixado >= lanc.Valor)
        {
            lanc.Status = LancamentoStatus.Pago;
            lanc.Realizado = true;
            lanc.DataPagamento = dataUltimaBaixa ?? lanc.DataPagamento ?? DateTime.UtcNow;
        }
        else if (totalBaixado > 0m)
        {
            lanc.Status = LancamentoStatus.Parcial;
            lanc.Realizado = false;
        }
        else
        {
            lanc.Status = lanc.DataVencimento.Date < DateTime.UtcNow.Date
                ? LancamentoStatus.Atrasado
                : LancamentoStatus.Pendente;
            lanc.Realizado = false;
            lanc.DataPagamento = null;
        }
        lanc.DataAtualizacao = DateTime.UtcNow;
    }

    private static LancamentoBaixaResponse MapToResponse(LancamentoBaixa b) => new()
    {
        Id = b.Id,
        LancamentoId = b.LancamentoId,
        Data = b.Data,
        Valor = b.Valor,
        ContaId = b.ContaId,
        ContaNome = b.Conta?.Nome,
        ComprovanteFinanceiroId = b.ComprovanteFinanceiroId,
        Observacoes = b.Observacoes
    };
}
