using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Infrastructure.Data;
using MeuCatalogo.Application.Interfaces;
using MeuCatalogo.Application.Services.Faturas;
using Microsoft.EntityFrameworkCore;

namespace MeuCatalogo.Application.Services;

public sealed class ContaService : IContaService
{
    private readonly ApplicationDbContext _db;

    public ContaService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ApiResponse<List<ContaResponse>>> GetAllAsync(string userId, bool incluirInativas = false)
    {
        var q = _db.Contas.Where(c => c.UserId == userId);
        if (!incluirInativas) q = q.Where(c => c.Ativo);
        var contas = await q.OrderBy(c => c.Ordem).ThenBy(c => c.Nome).ToListAsync();
        return ApiResponse<List<ContaResponse>>.Success(contas.Select(MapToResponse).ToList());
    }

    public async Task<ApiResponse<ContaResponse>> GetByIdAsync(Guid id, string userId)
    {
        var conta = await _db.Contas.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (conta == null) return ApiResponse<ContaResponse>.Error(ResponseType.NotFound, "Conta não encontrada");
        return ApiResponse<ContaResponse>.Success(MapToResponse(conta));
    }

    public async Task<ApiResponse<ContaResponse>> CreateAsync(ContaRequest request, string userId)
    {
        var validacao = Validar(request);
        if (validacao != null) return ApiResponse<ContaResponse>.Error(validacao);

        var conta = new Conta
        {
            UserId = userId,
            Nome = request.Nome.Trim(),
            Tipo = request.Tipo,
            Cor = string.IsNullOrWhiteSpace(request.Cor) ? "#3F51B5" : request.Cor,
            Ordem = request.Ordem,
            Limite = request.Tipo == ContaTipo.CartaoCredito ? request.Limite : null,
            DiaFechamento = request.Tipo == ContaTipo.CartaoCredito ? request.DiaFechamento : null,
            DiaVencimento = request.Tipo == ContaTipo.CartaoCredito ? request.DiaVencimento : null,
            SaldoInicial = request.Tipo == ContaTipo.CartaoCredito ? 0m : request.SaldoInicial
        };

        _db.Contas.Add(conta);
        await _db.SaveChangesAsync();
        return ApiResponse<ContaResponse>.Success(ResponseType.Created, MapToResponse(conta), "Conta criada");
    }

    public async Task<ApiResponse<ContaResponse>> UpdateAsync(Guid id, ContaRequest request, string userId)
    {
        var validacao = Validar(request);
        if (validacao != null) return ApiResponse<ContaResponse>.Error(validacao);

        var conta = await _db.Contas.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (conta == null) return ApiResponse<ContaResponse>.Error(ResponseType.NotFound, "Conta não encontrada");

        if (conta.Tipo != request.Tipo)
            return ApiResponse<ContaResponse>.Error("Não é possível alterar o tipo da conta");

        var diaFechamentoMudou = conta.DiaFechamento != request.DiaFechamento;
        var diaVencimentoMudou = conta.DiaVencimento != request.DiaVencimento;

        conta.Nome = request.Nome.Trim();
        conta.Cor = string.IsNullOrWhiteSpace(request.Cor) ? conta.Cor : request.Cor;
        conta.Ordem = request.Ordem;
        if (conta.EhCartaoCredito())
        {
            conta.Limite = request.Limite;
            conta.DiaFechamento = request.DiaFechamento;
            conta.DiaVencimento = request.DiaVencimento;
        }
        else
        {
            conta.SaldoInicial = request.SaldoInicial;
        }
        conta.DataAtualizacao = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        if (conta.EhCartaoCredito() && (diaFechamentoMudou || diaVencimentoMudou)
            && conta.DiaFechamento.HasValue && conta.DiaVencimento.HasValue)
        {
            await RecalcularFaturasFuturasAsync(conta);
        }

        return ApiResponse<ContaResponse>.Success(MapToResponse(conta));
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, string userId)
    {
        var conta = await _db.Contas.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (conta == null) return ApiResponse<bool>.Error(ResponseType.NotFound, "Conta não encontrada");

        var temLancamento = await _db.Lancamentos.AnyAsync(l => l.ContaId == id);
        if (temLancamento)
        {
            conta.Ativo = false;
            conta.DataAtualizacao = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Success(true, "Conta inativada (possui lançamentos vinculados)");
        }

        _db.Contas.Remove(conta);
        await _db.SaveChangesAsync();
        return ApiResponse<bool>.Success(true, "Conta removida");
    }

    public async Task<ApiResponse<bool>> SetAtivoAsync(Guid id, string userId, bool ativo)
    {
        var conta = await _db.Contas.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (conta == null) return ApiResponse<bool>.Error(ResponseType.NotFound, "Conta não encontrada");
        conta.Ativo = ativo;
        conta.DataAtualizacao = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ApiResponse<bool>.Success(true);
    }

    private async Task RecalcularFaturasFuturasAsync(Conta conta)
    {
        var hoje = DateTime.UtcNow.Date;
        var faturas = await _db.Faturas
            .Where(f => f.ContaId == conta.Id
                        && f.DataVencimento > hoje
                        && (f.ValorPago ?? 0m) == 0m)
            .ToListAsync();

        foreach (var f in faturas)
        {
            var (inicio, fim, venc) = FaturaCalculator.Calcular(
                conta.DiaFechamento!.Value,
                conta.DiaVencimento!.Value,
                f.Mes,
                f.Ano);
            f.DataInicio = inicio;
            f.DataFim = fim;
            f.DataVencimento = venc;
            f.DataAtualizacao = DateTime.UtcNow;
        }

        if (faturas.Count > 0) await _db.SaveChangesAsync();
    }

    private static string? Validar(ContaRequest r)
    {
        if (string.IsNullOrWhiteSpace(r.Nome)) return "Nome é obrigatório";
        if (r.Nome.Length > 80) return "Nome deve ter até 80 caracteres";
        if (r.Tipo == ContaTipo.CartaoCredito)
        {
            if (!r.DiaFechamento.HasValue || r.DiaFechamento < 1 || r.DiaFechamento > 31)
                return "Dia de fechamento deve estar entre 1 e 31";
            if (!r.DiaVencimento.HasValue || r.DiaVencimento < 1 || r.DiaVencimento > 31)
                return "Dia de vencimento deve estar entre 1 e 31";
            if (r.Limite.HasValue && r.Limite < 0)
                return "Limite deve ser positivo";
        }
        return null;
    }

    private static ContaResponse MapToResponse(Conta c) => new()
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
        Ativo = c.Ativo,
        CreatedAt = c.DataCriacao,
        UpdatedAt = c.DataAtualizacao
    };
}
