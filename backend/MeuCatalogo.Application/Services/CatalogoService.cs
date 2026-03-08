using MeuCatalogo.Application.DTOs;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Interfaces;
using MeuCatalogo.Application.Infrastructure.Data;
using MeuCatalogo.Application.Infrastructure.Data.Repository;
using MeuCatalogo.Application.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
using System.Text.RegularExpressions;

namespace MeuCatalogo.Application.Services;

public sealed class CatalogoService : ICatalogoService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IStorageService _storage;
    private readonly IMemoryCache _cache;

    public CatalogoService(ApplicationDbContext dbContext, IStorageService storage, IMemoryCache cache)
    {
        _dbContext = dbContext;
        _storage = storage;
        _cache = cache;
    }

    public async Task<ApiResponse<IEnumerable<CatalogoDto>>> ObterTodosPublicosAsync()
    {
        var catalogos = await _dbContext.Catalogos
            .AsNoTracking()
            .ToListAsync();

        var items = catalogos.Select(c => new CatalogoDto
        {
            Id = c.Id,
            Nome = c.Nome,
            Descricao = c.Descricao,
            DataCriacao = c.DataCriacao,
            DataAtualizacao = c.DataAtualizacao
        });

        return ApiResponse<IEnumerable<CatalogoDto>>.Success(items);
    }

    public async Task<ApiResponse<IEnumerable<CatalogoDto>>> ObterPorUsuarioIdAsync(string usuarioId)
    {
        var cacheKey = $"catalogos:usuario:{usuarioId}";
        if (_cache.TryGetValue(cacheKey, out List<CatalogoDto>? cachedCatalogos) && cachedCatalogos != null)
            return ApiResponse<IEnumerable<CatalogoDto>>.Success(cachedCatalogos);

        var catalogos = await _dbContext.ObterCatalogosPorUsuarioIdAsync(usuarioId);

        var items = catalogos.Select(c => new CatalogoDto
        {
            Id = c.Id,
            Nome = c.Nome,
            Descricao = c.Descricao,
            DataCriacao = c.DataCriacao,
            DataAtualizacao = c.DataAtualizacao
        }).ToList();

        _cache.Set(cacheKey, items, TimeSpan.FromSeconds(20));

        return ApiResponse<IEnumerable<CatalogoDto>>.Success(items);
    }

    public async Task<ApiResponse<CatalogoDto?>> ObterPorIdAsync(Guid id, string usuarioId)
    {
        var catalogo = await _dbContext.ObterCatalogoComProdutoAsync(id);
        if (catalogo == null)
            return ApiResponse<CatalogoDto?>.Error(ResponseType.NotFound, "Catálogo não encontrado.");

        if (catalogo.UserId != usuarioId)
            return ApiResponse<CatalogoDto?>.Error(ResponseType.Validation, "Você não tem permissão para acessar este catálogo.");

        return ApiResponse<CatalogoDto?>.Success(new CatalogoDto
        {
            Id = catalogo.Id,
            Nome = catalogo.Nome,
            Descricao = catalogo.Descricao,
            DataCriacao = catalogo.DataCriacao,
            DataAtualizacao = catalogo.DataAtualizacao,
            Produtos = catalogo.Produtos?.Select(p => new ProdutoDto
            {
                Id = p.Id,
                Nome = p.Nome,
                CategoriaId = p.CategoriaId,
                CategoriaNome = p.Categoria?.Nome,
                CatalogoId = p.CatalogoId,
                Preco = p.Preco,
                PrecoComDesconto = p.PrecoComDesconto,
                InformacoesAdicionais = p.InformacoesAdicionais,
                DataCriacao = p.DataCriacao,
                DataAtualizacao = p.DataAtualizacao,
                Estoque = p.Estoque != null ? new EstoqueDto
                {
                    Id = p.Estoque.Id,
                    ProdutoId = p.Estoque.ProdutoId,
                    Quantidade = p.Estoque.Quantidade,
                    QuantidadeMinima = p.Estoque.QuantidadeMinima,
                    QuantidadeMaxima = p.Estoque.QuantidadeMaxima,
                    DataCriacao = p.Estoque.DataCriacao,
                    DataAtualizacao = p.Estoque.DataAtualizacao
                } : null,
                Imagens = p.Imagens?.Select(img => new ProdutoImagemDto
                {
                    Id = img.Id,
                    Url = _storage.GetPresignedUrlFromPublicUrl(img.Url, TimeSpan.FromMinutes(60)),
                    IsPrincipal = img.IsPrincipal,
                    Ordem = img.Ordem
                }).OrderBy(i => i.Ordem).ToList() ?? new List<ProdutoImagemDto>(),
                Variacoes = p.Variacoes?.Select(v => new VariacaoDto
                {
                    Id = v.Id,
                    ProdutoId = v.ProdutoId,
                    TipoVariacaoId = v.TipoVariacaoId,
                    TipoNome = v.TipoVariacao?.Nome ?? "N/A",
                    OpcaoVariacaoId = v.OpcaoVariacaoId,
                    Valor = v.OpcaoVariacao?.Valor ?? "N/A",
                    DataCriacao = v.DataCriacao
                }).ToList() ?? new List<VariacaoDto>()
            }).ToList()
        });
    }


    public async Task<ApiResponse<Guid?>> ObterCatalogoIdAsync(Guid produtoId, string userId)
    {
        Guid? id = await _dbContext.Catalogos
            .Where(x => x.UserId == userId && x.Produtos.Any(produto => produto.Id == produtoId))
            .Select(x => x.Id)
            .FirstOrDefaultAsync();

        if (id == null)
            return ApiResponse<Guid?>.Error(ResponseType.NotFound, "Catálogo não encontrado para o produto especificado.");
        return ApiResponse<Guid?>.Success(id);
    }

    public async Task<ApiResponse<CatalogoDto>> AdicionarAsync(CatalogoCreateDto catalogoDto, string usuarioId)
    {
        var nomeCurtoNormalizado = NormalizarNomeCurto(catalogoDto.NomeCurto);

        if (nomeCurtoNormalizado != catalogoDto.NomeCurto)
            return ApiResponse<CatalogoDto>.Error(ResponseType.Validation, "O nome curto deve ser normalizado.");

        var user = await _dbContext.Users
            .Include(u => u.Assinaturas)
            .ThenInclude(a => a.PlanoAssinatura)
            .FirstOrDefaultAsync(u => u.Id == usuarioId);

        if (user == null)
            return ApiResponse<CatalogoDto>.Error(ResponseType.NotFound, "Usuário não encontrado.");

        int quantidadeAtual = await _dbContext.Catalogos.CountAsync(c => c.UserId == usuarioId);

        if (!user.PodeAdicionarCatalogo(quantidadeAtual))
            return ApiResponse<CatalogoDto>.Error(ResponseType.Validation, "Limite de catálogos do seu plano atingido. Faça um upgrade para continuar.");

        var catalogo = new Catalogo(
            nome: catalogoDto.Nome,
            descricao: catalogoDto.Descricao,
            nomeCurto: nomeCurtoNormalizado,
            email: catalogoDto.Email,
            numeroWhatsapp: catalogoDto.NumeroWhatsapp,
            userId: usuarioId
        );

        await _dbContext.AddCatalogoAsync(catalogo);

        return ApiResponse<CatalogoDto>.Success(catalogo.MapToDto(_storage));
    }

    public async Task<ApiResponse<CatalogoDto>> AtualizarAsync(Guid id, CatalogoUpdateDto catalogoDto, string usuarioId)
    {
        var catalogo = await _dbContext.ObterCatalogoPorIdAsync(id);

        if (catalogo == null)
            return ApiResponse<CatalogoDto>.Error(ResponseType.NotFound, "Catálogo não encontrado.");

        if (catalogo.UserId != usuarioId)
            throw new UnauthorizedAccessException("Você não tem permissão para atualizar este catálogo.");

        catalogo.Nome = catalogoDto.Nome;
        catalogo.Descricao = catalogoDto.Descricao;
        catalogo.NomeCurto = catalogoDto.NomeCurto;
        catalogo.Email = catalogoDto.Email;
        catalogo.NumeroWhatsapp = catalogoDto.NumeroWhatsapp;
        catalogo.DataAtualizacao = DateTime.UtcNow;

        await _dbContext.UpdateCatalogoAsync(catalogo);

        return ApiResponse<CatalogoDto>.Success(new CatalogoDto
        {
            Id = catalogo.Id,
            Nome = catalogo.Nome,
            Descricao = catalogo.Descricao,
            NomeCurto = catalogo.NomeCurto,
            Email = catalogo.Email,
            NumeroWhatsapp = catalogo.NumeroWhatsapp,
            DataCriacao = catalogo.DataCriacao,
            DataAtualizacao = catalogo.DataAtualizacao
        });
    }

    public async Task<ApiResponse<bool>> DeleteCatalogoAsync(Guid id, string usuarioId)
    {
        var catalogo = await _dbContext.ObterCatalogoPorIdAsync(id);

        if (catalogo == null)
            return ApiResponse<bool>.Error(ResponseType.NotFound, "Catálogo não encontrado.");

        if (catalogo.UserId != usuarioId)
            return ApiResponse<bool>.Error(ResponseType.Forbidden, "Você não tem permissão para excluir este catálogo.");

        await _dbContext.DeleteCatalogoAsync(id);

        return ApiResponse<bool>.Success(true);
    }

    public static string NormalizarNomeCurto(string input)
    {
        input = input.ToLowerInvariant().Trim();
        input = RemoveAcentos(input);
        input = Regex.Replace(input, @"[^a-z0-9\-]", "-");
        input = Regex.Replace(input, @"-+", "-");

        return input;
    }

    private static string RemoveAcentos(string texto)
    {
        if (string.IsNullOrEmpty(texto)) return texto;
        var normalized = texto.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var ch in normalized)
        {
            var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
