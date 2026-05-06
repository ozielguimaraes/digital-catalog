using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Infrastructure.Data;
using MeuCatalogo.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeuCatalogo.Application.Services;

public sealed class CategoriaFinanceiraService : ICategoriaFinanceiraService
{
    private readonly ApplicationDbContext _db;

    public CategoriaFinanceiraService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ApiResponse<List<CategoriaFinanceiraResponse>>> GetAllAsync(string userId, CategoriaFinanceiraTipo? tipo = null)
    {
        var q = _db.CategoriasFinanceiras
            .Include(c => c.Subcategorias.Where(s => s.Ativo))
            .Where(c => c.UserId == userId && c.Ativo);

        if (tipo.HasValue) q = q.Where(c => c.Tipo == tipo.Value);

        var lista = await q
            .OrderBy(c => c.Tipo).ThenBy(c => c.Ordem).ThenBy(c => c.Nome)
            .ToListAsync();

        return ApiResponse<List<CategoriaFinanceiraResponse>>.Success(lista.Select(MapToResponse).ToList());
    }

    public async Task<ApiResponse<CategoriaFinanceiraResponse>> GetByIdAsync(Guid id, string userId)
    {
        var cat = await _db.CategoriasFinanceiras
            .Include(c => c.Subcategorias.Where(s => s.Ativo))
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (cat == null) return ApiResponse<CategoriaFinanceiraResponse>.Error(ResponseType.NotFound, "Categoria não encontrada");
        return ApiResponse<CategoriaFinanceiraResponse>.Success(MapToResponse(cat));
    }

    public async Task<ApiResponse<CategoriaFinanceiraResponse>> CreateAsync(CategoriaFinanceiraRequest request, string userId)
    {
        var err = Validar(request);
        if (err != null) return ApiResponse<CategoriaFinanceiraResponse>.Error(err);

        var cat = new CategoriaFinanceira
        {
            UserId = userId,
            Tipo = request.Tipo,
            Nome = request.Nome.Trim(),
            IconeNome = string.IsNullOrWhiteSpace(request.IconeNome) ? "tag" : request.IconeNome.Trim(),
            Cor = string.IsNullOrWhiteSpace(request.Cor) ? "#9E9E9E" : request.Cor,
            Ordem = request.Ordem
        };

        _db.CategoriasFinanceiras.Add(cat);
        await _db.SaveChangesAsync();
        return ApiResponse<CategoriaFinanceiraResponse>.Success(ResponseType.Created, MapToResponse(cat), "Categoria criada");
    }

    public async Task<ApiResponse<CategoriaFinanceiraResponse>> UpdateAsync(Guid id, CategoriaFinanceiraRequest request, string userId)
    {
        var err = Validar(request);
        if (err != null) return ApiResponse<CategoriaFinanceiraResponse>.Error(err);

        var cat = await _db.CategoriasFinanceiras
            .Include(c => c.Subcategorias)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (cat == null) return ApiResponse<CategoriaFinanceiraResponse>.Error(ResponseType.NotFound, "Categoria não encontrada");

        cat.Tipo = request.Tipo;
        cat.Nome = request.Nome.Trim();
        cat.IconeNome = string.IsNullOrWhiteSpace(request.IconeNome) ? cat.IconeNome : request.IconeNome.Trim();
        cat.Cor = string.IsNullOrWhiteSpace(request.Cor) ? cat.Cor : request.Cor;
        cat.Ordem = request.Ordem;
        cat.DataAtualizacao = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ApiResponse<CategoriaFinanceiraResponse>.Success(MapToResponse(cat));
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, string userId)
    {
        var cat = await _db.CategoriasFinanceiras.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (cat == null) return ApiResponse<bool>.Error(ResponseType.NotFound, "Categoria não encontrada");

        var emUso = await _db.Lancamentos.AnyAsync(l => l.CategoriaFinanceiraId == id);
        if (emUso)
        {
            cat.Ativo = false;
            cat.DataAtualizacao = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Success(true, "Categoria inativada (em uso)");
        }

        _db.CategoriasFinanceiras.Remove(cat);
        await _db.SaveChangesAsync();
        return ApiResponse<bool>.Success(true);
    }

    public async Task<ApiResponse<SubcategoriaFinanceiraResponse>> CreateSubcategoriaAsync(Guid categoriaId, SubcategoriaFinanceiraRequest request, string userId)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
            return ApiResponse<SubcategoriaFinanceiraResponse>.Error("Nome é obrigatório");

        var cat = await _db.CategoriasFinanceiras.FirstOrDefaultAsync(c => c.Id == categoriaId && c.UserId == userId);
        if (cat == null) return ApiResponse<SubcategoriaFinanceiraResponse>.Error(ResponseType.NotFound, "Categoria não encontrada");

        var sub = new SubcategoriaFinanceira
        {
            CategoriaFinanceiraId = categoriaId,
            Nome = request.Nome.Trim(),
            IconeNome = request.IconeNome,
            Cor = request.Cor,
            Ordem = request.Ordem
        };

        _db.SubcategoriasFinanceiras.Add(sub);
        await _db.SaveChangesAsync();
        return ApiResponse<SubcategoriaFinanceiraResponse>.Success(ResponseType.Created, MapSub(sub), "Subcategoria criada");
    }

    public async Task<ApiResponse<SubcategoriaFinanceiraResponse>> UpdateSubcategoriaAsync(Guid categoriaId, Guid subcategoriaId, SubcategoriaFinanceiraRequest request, string userId)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
            return ApiResponse<SubcategoriaFinanceiraResponse>.Error("Nome é obrigatório");

        var sub = await _db.SubcategoriasFinanceiras
            .Include(s => s.CategoriaFinanceira)
            .FirstOrDefaultAsync(s => s.Id == subcategoriaId && s.CategoriaFinanceiraId == categoriaId);
        if (sub == null || sub.CategoriaFinanceira?.UserId != userId)
            return ApiResponse<SubcategoriaFinanceiraResponse>.Error(ResponseType.NotFound, "Subcategoria não encontrada");

        sub.Nome = request.Nome.Trim();
        sub.IconeNome = request.IconeNome;
        sub.Cor = request.Cor;
        sub.Ordem = request.Ordem;
        sub.DataAtualizacao = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ApiResponse<SubcategoriaFinanceiraResponse>.Success(MapSub(sub));
    }

    public async Task<ApiResponse<bool>> DeleteSubcategoriaAsync(Guid categoriaId, Guid subcategoriaId, string userId)
    {
        var sub = await _db.SubcategoriasFinanceiras
            .Include(s => s.CategoriaFinanceira)
            .FirstOrDefaultAsync(s => s.Id == subcategoriaId && s.CategoriaFinanceiraId == categoriaId);
        if (sub == null || sub.CategoriaFinanceira?.UserId != userId)
            return ApiResponse<bool>.Error(ResponseType.NotFound, "Subcategoria não encontrada");

        var emUso = await _db.Lancamentos.AnyAsync(l => l.SubcategoriaFinanceiraId == subcategoriaId);
        if (emUso)
        {
            sub.Ativo = false;
            sub.DataAtualizacao = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Success(true, "Subcategoria inativada (em uso)");
        }

        _db.SubcategoriasFinanceiras.Remove(sub);
        await _db.SaveChangesAsync();
        return ApiResponse<bool>.Success(true);
    }

    private static string? Validar(CategoriaFinanceiraRequest r)
    {
        if (string.IsNullOrWhiteSpace(r.Nome)) return "Nome é obrigatório";
        if (r.Nome.Length > 80) return "Nome deve ter até 80 caracteres";
        return null;
    }

    private static CategoriaFinanceiraResponse MapToResponse(CategoriaFinanceira c) => new()
    {
        Id = c.Id,
        Tipo = c.Tipo,
        Nome = c.Nome,
        IconeNome = c.IconeNome,
        Cor = c.Cor,
        Ordem = c.Ordem,
        Ativo = c.Ativo,
        Subcategorias = c.Subcategorias?
            .Where(s => s.Ativo)
            .OrderBy(s => s.Ordem).ThenBy(s => s.Nome)
            .Select(MapSub).ToList() ?? new()
    };

    private static SubcategoriaFinanceiraResponse MapSub(SubcategoriaFinanceira s) => new()
    {
        Id = s.Id,
        CategoriaFinanceiraId = s.CategoriaFinanceiraId,
        Nome = s.Nome,
        IconeNome = s.IconeNome,
        Cor = s.Cor,
        Ordem = s.Ordem,
        Ativo = s.Ativo
    };
}
