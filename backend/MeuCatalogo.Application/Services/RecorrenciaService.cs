using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Infrastructure.Data;
using MeuCatalogo.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeuCatalogo.Application.Services;

public sealed class RecorrenciaService : IRecorrenciaService
{
    private readonly ApplicationDbContext _db;

    public RecorrenciaService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ApiResponse<List<RecorrenciaResponse>>> GetAllAsync(string userId)
    {
        var lista = await _db.Recorrencias
            .Include(r => r.Conta)
            .Include(r => r.CategoriaFinanceira)
            .Include(r => r.SubcategoriaFinanceira)
            .Where(r => r.UserId == userId && r.Ativo)
            .OrderBy(r => r.Descricao)
            .ToListAsync();
        return ApiResponse<List<RecorrenciaResponse>>.Success(lista.Select(MapToResponse).ToList());
    }

    public async Task<ApiResponse<RecorrenciaResponse>> GetByIdAsync(Guid id, string userId)
    {
        var r = await Carregar(id, userId);
        if (r == null) return ApiResponse<RecorrenciaResponse>.Error(ResponseType.NotFound, "Recorrência não encontrada");
        return ApiResponse<RecorrenciaResponse>.Success(MapToResponse(r));
    }

    public async Task<ApiResponse<RecorrenciaResponse>> CreateAsync(RecorrenciaRequest request, string userId)
    {
        var err = await ValidarAsync(request, userId);
        if (err != null) return ApiResponse<RecorrenciaResponse>.Error(err);

        var rec = new Recorrencia
        {
            UserId = userId,
            Tipo = request.Tipo,
            Descricao = request.Descricao.Trim(),
            ContaId = request.ContaId,
            CategoriaFinanceiraId = request.CategoriaFinanceiraId,
            SubcategoriaFinanceiraId = request.SubcategoriaFinanceiraId,
            ValorPadrao = request.ValorPadrao,
            Periodicidade = request.Periodicidade,
            DiaDoMes = request.DiaDoMes,
            DiaDaSemana = request.DiaDaSemana,
            DataInicio = request.DataInicio,
            DataFim = request.DataFim,
            ProximaData = request.DataInicio
        };

        _db.Recorrencias.Add(rec);
        await _db.SaveChangesAsync();
        var carregada = await Carregar(rec.Id, userId);
        return ApiResponse<RecorrenciaResponse>.Success(ResponseType.Created, MapToResponse(carregada!), "Recorrência criada");
    }

    public async Task<ApiResponse<RecorrenciaResponse>> UpdateAsync(Guid id, RecorrenciaRequest request, string userId)
    {
        var err = await ValidarAsync(request, userId);
        if (err != null) return ApiResponse<RecorrenciaResponse>.Error(err);

        var rec = await _db.Recorrencias.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
        if (rec == null) return ApiResponse<RecorrenciaResponse>.Error(ResponseType.NotFound, "Recorrência não encontrada");

        rec.Tipo = request.Tipo;
        rec.Descricao = request.Descricao.Trim();
        rec.ContaId = request.ContaId;
        rec.CategoriaFinanceiraId = request.CategoriaFinanceiraId;
        rec.SubcategoriaFinanceiraId = request.SubcategoriaFinanceiraId;
        rec.ValorPadrao = request.ValorPadrao;
        rec.Periodicidade = request.Periodicidade;
        rec.DiaDoMes = request.DiaDoMes;
        rec.DiaDaSemana = request.DiaDaSemana;
        rec.DataInicio = request.DataInicio;
        rec.DataFim = request.DataFim;
        rec.DataAtualizacao = DateTime.UtcNow;
        if (rec.ProximaData < request.DataInicio) rec.ProximaData = request.DataInicio;
        await _db.SaveChangesAsync();

        var carregada = await Carregar(rec.Id, userId);
        return ApiResponse<RecorrenciaResponse>.Success(MapToResponse(carregada!));
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, string userId)
    {
        var rec = await _db.Recorrencias.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
        if (rec == null) return ApiResponse<bool>.Error(ResponseType.NotFound, "Recorrência não encontrada");
        rec.Ativo = false;
        rec.DataAtualizacao = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ApiResponse<bool>.Success(true);
    }

    public async Task<ApiResponse<bool>> SetAtivoAsync(Guid id, string userId, bool ativo)
    {
        var rec = await _db.Recorrencias.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
        if (rec == null) return ApiResponse<bool>.Error(ResponseType.NotFound, "Recorrência não encontrada");
        rec.Ativo = ativo;
        rec.DataAtualizacao = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ApiResponse<bool>.Success(true);
    }

    public async Task<int> GerarPendentesAsync(string userId, DateTime ateData)
    {
        var recorrencias = await _db.Recorrencias
            .Where(r => r.UserId == userId && r.Ativo)
            .ToListAsync();

        var totalGerado = 0;
        foreach (var r in recorrencias)
        {
            while (r.ProximaData <= ateData && (r.DataFim == null || r.ProximaData <= r.DataFim))
            {
                var dataAlvo = r.ProximaData;
                var jaExiste = await _db.Lancamentos
                    .AnyAsync(l => l.RecorrenciaId == r.Id && l.DataVencimento == dataAlvo);

                if (!jaExiste)
                {
                    var tipoLancamento = r.Tipo == CategoriaFinanceiraTipo.Receita
                        ? LancamentoTipo.Receber
                        : LancamentoTipo.Pagar;

                    _db.Lancamentos.Add(new Lancamento
                    {
                        UserId = userId,
                        Descricao = r.Descricao,
                        Valor = r.ValorPadrao ?? 0m,
                        DataVencimento = dataAlvo,
                        Tipo = tipoLancamento,
                        Status = LancamentoStatus.Pendente,
                        ContaId = r.ContaId,
                        CategoriaFinanceiraId = r.CategoriaFinanceiraId,
                        SubcategoriaFinanceiraId = r.SubcategoriaFinanceiraId,
                        Operacao = LancamentoOperacao.Simples,
                        RecorrenciaId = r.Id,
                        Realizado = false
                    });
                    totalGerado++;
                }

                r.ProximaData = ProximaOcorrencia(r);
            }
        }
        if (totalGerado > 0) await _db.SaveChangesAsync();
        return totalGerado;
    }

    private static DateTime ProximaOcorrencia(Recorrencia r) => r.Periodicidade switch
    {
        RecorrenciaPeriodicidade.Mensal => r.ProximaData.AddMonths(1),
        RecorrenciaPeriodicidade.Semanal => r.ProximaData.AddDays(7),
        RecorrenciaPeriodicidade.Anual => r.ProximaData.AddYears(1),
        _ => r.ProximaData.AddMonths(1)
    };

    private async Task<Recorrencia?> Carregar(Guid id, string userId) =>
        await _db.Recorrencias
            .Include(r => r.Conta)
            .Include(r => r.CategoriaFinanceira)
            .Include(r => r.SubcategoriaFinanceira)
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

    private async Task<string?> ValidarAsync(RecorrenciaRequest r, string userId)
    {
        if (string.IsNullOrWhiteSpace(r.Descricao)) return "Descrição é obrigatória";
        if (r.Descricao.Length > 150) return "Descrição deve ter até 150 caracteres";
        if (r.ValorPadrao.HasValue && r.ValorPadrao < 0) return "Valor padrão não pode ser negativo";
        if (r.DataFim.HasValue && r.DataFim < r.DataInicio) return "Data fim deve ser posterior à inicial";
        if (r.Periodicidade == RecorrenciaPeriodicidade.Mensal && (!r.DiaDoMes.HasValue || r.DiaDoMes < 1 || r.DiaDoMes > 31))
            return "Dia do mês obrigatório para recorrência mensal";

        var conta = await _db.Contas.AnyAsync(c => c.Id == r.ContaId && c.UserId == userId);
        if (!conta) return "Conta não encontrada";
        var cat = await _db.CategoriasFinanceiras.AnyAsync(c => c.Id == r.CategoriaFinanceiraId && c.UserId == userId);
        if (!cat) return "Categoria não encontrada";
        return null;
    }

    private static RecorrenciaResponse MapToResponse(Recorrencia r) => new()
    {
        Id = r.Id,
        Tipo = r.Tipo,
        Descricao = r.Descricao,
        ContaId = r.ContaId,
        ContaNome = r.Conta?.Nome,
        CategoriaFinanceiraId = r.CategoriaFinanceiraId,
        CategoriaFinanceiraNome = r.CategoriaFinanceira?.Nome,
        SubcategoriaFinanceiraId = r.SubcategoriaFinanceiraId,
        SubcategoriaFinanceiraNome = r.SubcategoriaFinanceira?.Nome,
        ValorPadrao = r.ValorPadrao,
        Periodicidade = r.Periodicidade,
        DiaDoMes = r.DiaDoMes,
        DiaDaSemana = r.DiaDaSemana,
        DataInicio = r.DataInicio,
        DataFim = r.DataFim,
        ProximaData = r.ProximaData,
        Ativo = r.Ativo
    };
}
