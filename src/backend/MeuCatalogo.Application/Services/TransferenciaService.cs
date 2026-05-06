using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Infrastructure.Data;
using MeuCatalogo.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeuCatalogo.Application.Services;

public sealed class TransferenciaService : ITransferenciaService
{
    private readonly ApplicationDbContext _db;
    private readonly IFaturaService _faturaService;

    public TransferenciaService(ApplicationDbContext db, IFaturaService faturaService)
    {
        _db = db;
        _faturaService = faturaService;
    }

    public async Task<ApiResponse<List<LancamentoResponse>>> CriarEntreContasAsync(TransferenciaRequest request, string userId)
    {
        var validacao = ValidarBasico(request);
        if (validacao != null) return ApiResponse<List<LancamentoResponse>>.Error(validacao);

        var (origem, destino, err) = await ResolverContasAsync(request.ContaOrigemId, request.ContaDestinoId, userId);
        if (err != null) return ApiResponse<List<LancamentoResponse>>.Error(err);

        if (origem!.EhCartaoCredito())
            return ApiResponse<List<LancamentoResponse>>.Error("Cartão de crédito não pode ser conta de origem em transferência. Use 'Pagamento de fatura'.");
        if (destino!.EhCartaoCredito())
            return ApiResponse<List<LancamentoResponse>>.Error("Cartão de crédito como destino apenas em pagamento de fatura.");

        var descricao = string.IsNullOrWhiteSpace(request.Descricao)
            ? $"Transferência {origem.Nome} → {destino.Nome}"
            : request.Descricao!.Trim();

        var saida = new Lancamento
        {
            UserId = userId,
            ContaId = origem.Id,
            Tipo = LancamentoTipo.Pagar,
            Operacao = LancamentoOperacao.Transferencia,
            TipoTransferencia = LancamentoTipoTransferencia.EntreContas,
            Descricao = descricao,
            Valor = request.Valor,
            DataVencimento = request.Data,
            DataPagamento = request.Data,
            Status = LancamentoStatus.Pago,
            Realizado = true
        };
        var entrada = new Lancamento
        {
            UserId = userId,
            ContaId = destino.Id,
            Tipo = LancamentoTipo.Receber,
            Operacao = LancamentoOperacao.Transferencia,
            TipoTransferencia = LancamentoTipoTransferencia.EntreContas,
            Descricao = descricao,
            Valor = request.Valor,
            DataVencimento = request.Data,
            DataPagamento = request.Data,
            Status = LancamentoStatus.Pago,
            Realizado = true
        };

        _db.Lancamentos.AddRange(saida, entrada);
        await _db.SaveChangesAsync();

        saida.LancamentoTransferenciaId = entrada.Id;
        entrada.LancamentoTransferenciaId = saida.Id;
        await _db.SaveChangesAsync();

        return ApiResponse<List<LancamentoResponse>>.Success(ResponseType.Created,
            new List<LancamentoResponse> { LancamentoMapper.MapToResponse(saida), LancamentoMapper.MapToResponse(entrada) },
            "Transferência registrada");
    }

    public async Task<ApiResponse<List<LancamentoResponse>>> CriarPagamentoFaturaAsync(TransferenciaRequest request, string userId)
    {
        var validacao = ValidarBasico(request);
        if (validacao != null) return ApiResponse<List<LancamentoResponse>>.Error(validacao);
        if (!request.FaturaMes.HasValue || !request.FaturaAno.HasValue)
            return ApiResponse<List<LancamentoResponse>>.Error("Mês e ano da fatura são obrigatórios");

        var (origem, destino, err) = await ResolverContasAsync(request.ContaOrigemId, request.ContaDestinoId, userId);
        if (err != null) return ApiResponse<List<LancamentoResponse>>.Error(err);

        if (origem!.EhCartaoCredito())
            return ApiResponse<List<LancamentoResponse>>.Error("Origem do pagamento não pode ser cartão de crédito");
        if (!destino!.EhCartaoCredito())
            return ApiResponse<List<LancamentoResponse>>.Error("Destino do pagamento deve ser um cartão de crédito");

        var fatura = await _faturaService.ObterOuCriarAsync(destino, request.FaturaMes.Value, request.FaturaAno.Value);
        var descricao = string.IsNullOrWhiteSpace(request.Descricao)
            ? $"Pagamento Fatura {destino.Nome} {request.FaturaMes:00}/{request.FaturaAno}"
            : request.Descricao!.Trim();

        var saida = new Lancamento
        {
            UserId = userId,
            ContaId = origem.Id,
            Tipo = LancamentoTipo.Pagar,
            Operacao = LancamentoOperacao.Transferencia,
            TipoTransferencia = LancamentoTipoTransferencia.PagamentoFatura,
            Descricao = descricao,
            Valor = request.Valor,
            DataVencimento = request.Data,
            DataPagamento = request.Data,
            Status = LancamentoStatus.Pago,
            Realizado = true,
            FaturaId = fatura.Id
        };
        var entrada = new Lancamento
        {
            UserId = userId,
            ContaId = destino.Id,
            Tipo = LancamentoTipo.Receber,
            Operacao = LancamentoOperacao.Transferencia,
            TipoTransferencia = LancamentoTipoTransferencia.PagamentoFatura,
            Descricao = descricao,
            Valor = request.Valor,
            DataVencimento = request.Data,
            DataPagamento = request.Data,
            Status = LancamentoStatus.Pago,
            Realizado = true,
            FaturaId = fatura.Id
        };

        _db.Lancamentos.AddRange(saida, entrada);
        await _db.SaveChangesAsync();

        saida.LancamentoTransferenciaId = entrada.Id;
        entrada.LancamentoTransferenciaId = saida.Id;

        fatura.ValorPago = (fatura.ValorPago ?? 0m) + request.Valor;
        fatura.DataAtualizacao = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return ApiResponse<List<LancamentoResponse>>.Success(ResponseType.Created,
            new List<LancamentoResponse> { LancamentoMapper.MapToResponse(saida), LancamentoMapper.MapToResponse(entrada) },
            "Pagamento de fatura registrado");
    }

    public async Task<ApiResponse<bool>> ExcluirAsync(Guid lancamentoId, string userId)
    {
        var lanc = await _db.Lancamentos
            .Include(l => l.LancamentoTransferencia)
            .FirstOrDefaultAsync(l => l.Id == lancamentoId && l.UserId == userId);
        if (lanc == null || lanc.Operacao != LancamentoOperacao.Transferencia)
            return ApiResponse<bool>.Error(ResponseType.NotFound, "Transferência não encontrada");

        var par = lanc.LancamentoTransferenciaId.HasValue
            ? await _db.Lancamentos.FirstOrDefaultAsync(l => l.Id == lanc.LancamentoTransferenciaId.Value)
            : null;

        if (lanc.TipoTransferencia == LancamentoTipoTransferencia.PagamentoFatura && lanc.FaturaId.HasValue)
        {
            var fatura = await _db.Faturas.FindAsync(lanc.FaturaId.Value);
            if (fatura != null)
            {
                fatura.ValorPago = Math.Max(0m, (fatura.ValorPago ?? 0m) - lanc.Valor);
                fatura.DataAtualizacao = DateTime.UtcNow;
            }
        }

        lanc.LancamentoTransferenciaId = null;
        if (par != null) par.LancamentoTransferenciaId = null;
        await _db.SaveChangesAsync();

        _db.Lancamentos.Remove(lanc);
        if (par != null) _db.Lancamentos.Remove(par);
        await _db.SaveChangesAsync();
        return ApiResponse<bool>.Success(true);
    }

    private async Task<(Conta? origem, Conta? destino, string? err)> ResolverContasAsync(Guid origemId, Guid destinoId, string userId)
    {
        if (origemId == destinoId) return (null, null, "Conta de origem e destino devem ser diferentes");
        var origem = await _db.Contas.FirstOrDefaultAsync(c => c.Id == origemId && c.UserId == userId && c.Ativo);
        var destino = await _db.Contas.FirstOrDefaultAsync(c => c.Id == destinoId && c.UserId == userId && c.Ativo);
        if (origem == null) return (null, null, "Conta de origem não encontrada");
        if (destino == null) return (null, null, "Conta de destino não encontrada");
        return (origem, destino, null);
    }

    private static string? ValidarBasico(TransferenciaRequest r)
    {
        if (r.Valor <= 0) return "Valor deve ser maior que zero";
        if (r.Data == default) return "Data é obrigatória";
        return null;
    }
}
