using MeuCatalogo.Application.DTOs;
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

    public async Task<IEnumerable<CatalogoDto>> GetCatalogosByUserIdAsync(string usuarioId)
    {
        var catalogos = await _dbContext.ObterCatalogosPorUsuarioIdAsync(usuarioId);

        return catalogos.Select(c => new CatalogoDto
        {
            Id = c.Id,
            Nome = c.Nome,
            Descricao = c.Descricao,
            DataCriacao = c.DataCriacao,
            DataAtualizacao = c.DataAtualizacao
        });
    }

    public async Task<CatalogoDto?> ObterCatalogoPorIdAsync(Guid id, string usuarioId)
    {
        var catalogo = await _dbContext.GetCatalogoWithProdutosAsync(id);
        if (catalogo == null || catalogo.UserId != usuarioId)
        {
            throw new UnauthorizedAccessException("Você não tem permissão para acessar este catálogo.");
        }

        return new CatalogoDto
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
        };
    }

    public async Task<CatalogoDto> CreateCatalogoAsync(CatalogoCreateDto catalogoDto, string usuarioId)
    {
        var catalogo = new Catalogo
        {
            Nome = catalogoDto.Nome,
            Descricao = catalogoDto.Descricao,
            UserId = usuarioId
        };

        await _dbContext.AddCatalogoAsync(catalogo);

        return catalogo.MapToDto();
    }

    public async Task<CatalogoDto?> UpdateCatalogoAsync(Guid id, CatalogoUpdateDto catalogoDto, string usuarioId)
    {
        var catalogo = await _dbContext.GetCatalogoByIdAsync(id);
        if (catalogo == null || catalogo.UserId != usuarioId)
        {
            throw new UnauthorizedAccessException("Você não tem permissão para atualizar este catálogo.");
        }

        catalogo.Nome = catalogoDto.Nome;
        catalogo.Descricao = catalogoDto.Descricao;

        await _dbContext.UpdateCatalogoAsync(catalogo);

        return new CatalogoDto
        {
            Id = catalogo.Id,
            Nome = catalogo.Nome,
            Descricao = catalogo.Descricao,
            DataCriacao = catalogo.DataCriacao,
            DataAtualizacao = catalogo.DataAtualizacao
        };
    }

    public async Task DeleteCatalogoAsync(Guid id, string usuarioId)
    {
        var catalogo = await _dbContext.GetCatalogoByIdAsync(id);
        if (catalogo == null || catalogo.UserId != usuarioId)
        {
            throw new UnauthorizedAccessException("Você não tem permissão para excluir este catálogo.");
        }

        await _dbContext.DeleteCatalogoAsync(id);
    }
}
