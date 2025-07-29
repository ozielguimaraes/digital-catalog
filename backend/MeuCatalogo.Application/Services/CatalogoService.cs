using MeuCatalogo.Application.DTOs;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Interfaces;
using MeuCatalogo.Application.Infrastructure.Data;
using MeuCatalogo.Application.Infrastructure.Data.Repository;
using MeuCatalogo.Application.Infrastructure.Mappers;

namespace MeuCatalogo.Application.Services;

public class CatalogoService : ICatalogoService
{
    private readonly ApplicationDbContext _dbContext;

    public CatalogoService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApiResponse<IEnumerable<CatalogoDto>>> ObterPorUsuarioIdAsync(string usuarioId)
    {
        var catalogos = await _dbContext.ObterCatalogosPorUsuarioIdAsync(usuarioId);

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

    public async Task<ApiResponse<CatalogoDto?>> ObterPorIdAsync(Guid id, string usuarioId)
    {
        var catalogo = await _dbContext.GetCatalogoWithProdutosAsync(id);
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
                } : null
            }).ToList()
        });
    }

    public async Task<ApiResponse<CatalogoDto>> AdicionarAsync(CatalogoCreateDto catalogoDto, string usuarioId)
    {
        var catalogo = new Catalogo(
            nome: catalogoDto.Nome,
            descricao: catalogoDto.Descricao,
            nomeCurto: catalogoDto.NomeCurto,
            email: catalogoDto.Email,
            numeroWhatsapp: catalogoDto.NumeroWhatsapp,
            userId: usuarioId
        );

        await _dbContext.AddCatalogoAsync(catalogo);

        return ApiResponse<CatalogoDto>.Success(catalogo.MapToDto());
    }

    public async Task<ApiResponse<CatalogoDto>> AtualizarAsync(Guid id, CatalogoUpdateDto catalogoDto, string usuarioId)
    {
        var catalogo = await _dbContext.GetCatalogoByIdAsync(id);

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
        var catalogo = await _dbContext.GetCatalogoByIdAsync(id);

        if (catalogo == null)
            return ApiResponse<bool>.Error(ResponseType.NotFound, "Catálogo não encontrado.");

        if (catalogo.UserId != usuarioId)
            return ApiResponse<bool>.Error(ResponseType.Forbidden, "Você não tem permissão para excluir este catálogo.");

        await _dbContext.DeleteCatalogoAsync(id);

        return ApiResponse<bool>.Success(true);
    }
}
