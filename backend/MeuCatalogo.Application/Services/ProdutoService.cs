using MeuCatalogo.Application.DTOs;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Interfaces;
using MeuCatalogo.Application.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using MeuCatalogo.Application.Infrastructure.Data.Repository;
using MeuCatalogo.Application.Infrastructure.Mappers;

namespace MeuCatalogo.Application.Services;

public class ProdutoService : IProdutoService
{
    private readonly ILogger<ProdutoService> _logger;
    private readonly ApplicationDbContext _dbContext;

    public ProdutoService(ApplicationDbContext dbContext, ILogger<ProdutoService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        _logger.LogInformation("Serviço de Produto inicializado");
    }

    public async Task<IEnumerable<ProdutoDto>> GetProdutosByCatalogoIdAsync(Guid catalogoId, string userId)
    {
        _logger.LogInformation("Iniciando {Metodo} para catalogoId: {CatalogoId}, usuarioId: {UsuarioId}",
            nameof(GetProdutosByCatalogoIdAsync), catalogoId, userId);

        var catalogo = await _dbContext.GetCatalogoByIdAsync(catalogoId);
        if (catalogo == null || catalogo.UserId != userId)
        {
            _logger.LogWarning("Acesso não autorizado ao catálogo {CatalogoId} pelo usuário {UsuarioId}",
                catalogoId, userId);
            throw new UnauthorizedAccessException("Você não tem permissão para acessar este catálogo.");
        }

        _logger.LogDebug("Buscando produtos para o catálogo {CatalogoId}", catalogoId);
        var produtos = await _dbContext.GetProdutosByCatalogoIdAsync(catalogoId);
        _logger.LogInformation("Encontrados {Quantidade} produtos para o catálogo {CatalogoId}",
            produtos.Count, catalogoId);

        return produtos.Select(p => new ProdutoDto
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
        });
    }

    public async Task<ProdutoDto?> GetProdutoByIdAsync(Guid id, string userId)
    {
        _logger.LogInformation("Iniciando {Metodo} para produtoId: {ProdutoId}, usuarioId: {UsuarioId}",
            nameof(GetProdutoByIdAsync), id, userId);

        var produto = await _dbContext.ObterProdutoComDetalhesAsync(id);
        if (produto == null)
        {
            _logger.LogWarning("Produto não encontrado com id: {ProdutoId}", id);
            throw new KeyNotFoundException("Produto não encontrado.");
        }

        _logger.LogDebug("Verificando permissões para o produto {ProdutoId}, catalogoId: {CatalogoId}",
            id, produto.CatalogoId);
        var catalogo = await _dbContext.GetCatalogoByIdAsync(produto.CatalogoId);
        if (catalogo == null || catalogo.UserId != userId)
        {
            _logger.LogWarning("Acesso não autorizado ao produto {ProdutoId} pelo usuário {UsuarioId}",
                id, userId);
            throw new UnauthorizedAccessException("Você não tem permissão para acessar este produto.");
        }

        _logger.LogInformation("Retornando detalhes do produto {ProdutoId}", id);
        return new ProdutoDto
        {
            Id = produto.Id,
            Nome = produto.Nome,
            CategoriaId = produto.CategoriaId,
            CategoriaNome = produto.Categoria?.Nome,
            CatalogoId = produto.CatalogoId,
            Preco = produto.Preco,
            PrecoComDesconto = produto.PrecoComDesconto,
            InformacoesAdicionais = produto.InformacoesAdicionais,
            DataCriacao = produto.DataCriacao,
            DataAtualizacao = produto.DataAtualizacao,
            Estoque = produto.Estoque != null ? new EstoqueDto
            {
                Id = produto.Estoque.Id,
                ProdutoId = produto.Estoque.ProdutoId,
                Quantidade = produto.Estoque.Quantidade,
                QuantidadeMinima = produto.Estoque.QuantidadeMinima,
                QuantidadeMaxima = produto.Estoque.QuantidadeMaxima,
                DataCriacao = produto.Estoque.DataCriacao,
                DataAtualizacao = produto.Estoque.DataAtualizacao
            } : null,
            Variacoes = produto.Variacoes.Select(v => new VariacaoDto
            {
                Id = v.Id,
                Nome = v.TipoVariacao.Nome,
                OpcaoVariacao = v.OpcaoVariacao.MapToDto(),
                ProdutoId = v.ProdutoId,
                DataCriacao = v.DataCriacao,
                DataAtualizacao = v.DataAtualizacao
            }).ToList()
        };
    }

    public async Task<ProdutoDto> CreateProdutoAsync(ProdutoCreateDto produtoDto, string userId)
    {
        _logger.LogInformation("Iniciando {Metodo} para catalogoId: {CatalogoId}, usuarioId: {UsuarioId}",
            nameof(CreateProdutoAsync), produtoDto.CatalogoId, userId);

        var catalogo = await _dbContext.GetCatalogoByIdAsync(produtoDto.CatalogoId);
        if (catalogo == null || catalogo.UserId != userId)
        {
            _logger.LogWarning("Acesso não autorizado para criar produto no catálogo {CatalogoId} pelo usuário {UsuarioId}",
                produtoDto.CatalogoId, userId);
            throw new UnauthorizedAccessException("Você não tem permissão para adicionar produtos a este catálogo.");
        }

        _logger.LogDebug("Criando novo produto para o catálogo {CatalogoId}", produtoDto.CatalogoId);
        var produto = new Produto
        {
            Nome = produtoDto.Nome,
            CategoriaId = produtoDto.CategoriaId,
            CatalogoId = produtoDto.CatalogoId,
            Preco = produtoDto.Preco,
            PrecoComDesconto = produtoDto.PrecoComDesconto,
            InformacoesAdicionais = produtoDto.InformacoesAdicionais
        };

        await _dbContext.AdicionarAsync(produto);
        _logger.LogInformation("Produto criado com sucesso. Id: {ProdutoId}", produto.Id);

        if (produtoDto.Estoque != null)
        {
            _logger.LogDebug("Adicionando informações de estoque para o produto {ProdutoId}", produto.Id);
            var estoque = new Estoque
            {
                ProdutoId = produto.Id,
                Quantidade = produtoDto.Estoque.Quantidade,
                QuantidadeMinima = produtoDto.Estoque.QuantidadeMinima,
                QuantidadeMaxima = produtoDto.Estoque.QuantidadeMaxima
            };

            produto.Estoque = estoque;
            await _dbContext.AtualizarAsync(produto);
            _logger.LogInformation("Estoque adicionado ao produto {ProdutoId}", produto.Id);
        }

        return new ProdutoDto
        {
            Id = produto.Id,
            Nome = produto.Nome,
            CategoriaId = produto.CategoriaId,
            CatalogoId = produto.CatalogoId,
            Preco = produto.Preco,
            PrecoComDesconto = produto.PrecoComDesconto,
            InformacoesAdicionais = produto.InformacoesAdicionais,
            DataCriacao = produto.DataCriacao,
            DataAtualizacao = produto.DataAtualizacao,
            Estoque = produto.Estoque != null ? new EstoqueDto
            {
                Id = produto.Estoque.Id,
                ProdutoId = produto.Estoque.ProdutoId,
                Quantidade = produto.Estoque.Quantidade,
                QuantidadeMinima = produto.Estoque.QuantidadeMinima,
                QuantidadeMaxima = produto.Estoque.QuantidadeMaxima,
                DataCriacao = produto.Estoque.DataCriacao,
                DataAtualizacao = produto.Estoque.DataAtualizacao
            } : null
        };
    }

    public async Task<ProdutoDto> UpdateProdutoAsync(Guid id, ProdutoUpdateDto produtoDto, string userId)
    {
        _logger.LogInformation("Iniciando {Metodo} para produtoId: {ProdutoId}, usuarioId: {UsuarioId}",
            nameof(UpdateProdutoAsync), id, userId);

        var produto = await _dbContext.ObterProdutoPorIdAsync(id);
        if (produto == null)
        {
            _logger.LogWarning("Produto não encontrado com id: {ProdutoId}", id);
            throw new KeyNotFoundException("Produto não encontrado.");
        }

        _logger.LogDebug("Verificando permissões para atualizar o produto {ProdutoId}, catalogoId: {CatalogoId}",
            id, produto.CatalogoId);
        var catalogo = await _dbContext.GetCatalogoByIdAsync(produto.CatalogoId);
        if (catalogo == null || catalogo.UserId != userId)
        {
            _logger.LogWarning("Acesso não autorizado para atualizar o produto {ProdutoId} pelo usuário {UsuarioId}",
                id, userId);
            throw new UnauthorizedAccessException("Você não tem permissão para atualizar este produto.");
        }

        _logger.LogDebug("Atualizando dados do produto {ProdutoId}", id);
        produto.Nome = produtoDto.Nome;
        produto.CategoriaId = produtoDto.CategoriaId;
        produto.Preco = produtoDto.Preco;
        produto.PrecoComDesconto = produtoDto.PrecoComDesconto;
        produto.InformacoesAdicionais = produtoDto.InformacoesAdicionais;

        await _dbContext.AtualizarProdutoAsync(produto);
        _logger.LogInformation("Produto {ProdutoId} atualizado com sucesso", id);

        return new ProdutoDto
        {
            Id = produto.Id,
            Nome = produto.Nome,
            CategoriaId = produto.CategoriaId,
            CatalogoId = produto.CatalogoId,
            Preco = produto.Preco,
            PrecoComDesconto = produto.PrecoComDesconto,
            InformacoesAdicionais = produto.InformacoesAdicionais,
            DataCriacao = produto.DataCriacao,
            DataAtualizacao = produto.DataAtualizacao
        };
    }

    public async Task DeleteProdutoAsync(Guid id, string userId)
    {
        _logger.LogInformation("Iniciando {Metodo} para produtoId: {ProdutoId}, usuarioId: {UsuarioId}",
            nameof(DeleteProdutoAsync), id, userId);

        var produto = await _dbContext.GetProdutoByIdAsync(id);
        if (produto == null)
        {
            _logger.LogWarning("Produto não encontrado com id: {ProdutoId}", id);
            throw new KeyNotFoundException("Produto não encontrado.");
        }

        _logger.LogDebug("Verificando permissões para excluir o produto {ProdutoId}, catalogoId: {CatalogoId}",
            id, produto.CatalogoId);
        var catalogo = await _dbContext.GetCatalogoByIdAsync(produto.CatalogoId);
        if (catalogo == null || catalogo.UserId != userId)
        {
            _logger.LogWarning("Acesso não autorizado para excluir o produto {ProdutoId} pelo usuário {UsuarioId}",
                id, userId);
            throw new UnauthorizedAccessException("Você não tem permissão para excluir este produto.");
        }

        _logger.LogDebug("Excluindo produto {ProdutoId}", id);
        await _dbContext.RemoverProdutoAsync(id);
        _logger.LogInformation("Produto {ProdutoId} excluído com sucesso", id);
    }

    public async Task<EstoqueDto> UpdateEstoqueAsync(Guid produtoId, EstoqueUpdateDto estoqueDto, string userId)
    {
        _logger.LogInformation("Iniciando {Metodo} para produtoId: {ProdutoId}, usuarioId: {UsuarioId}",
            nameof(UpdateEstoqueAsync), produtoId, userId);

        var produto = await _dbContext.GetProdutoWithDetailsAsync(produtoId);
        if (produto == null)
        {
            _logger.LogWarning("Produto não encontrado com id: {ProdutoId}", produtoId);
            throw new KeyNotFoundException("Produto não encontrado.");
        }

        _logger.LogDebug("Verificando permissões para atualizar estoque do produto {ProdutoId}, catalogoId: {CatalogoId}",
            produtoId, produto.CatalogoId);
        var catalogo = await _dbContext.GetCatalogoByIdAsync(produto.CatalogoId);
        if (catalogo == null || catalogo.UserId != userId)
        {
            _logger.LogWarning("Acesso não autorizado para atualizar estoque do produto {ProdutoId} pelo usuário {UsuarioId}",
                produtoId, userId);
            throw new UnauthorizedAccessException("Você não tem permissão para atualizar o estoque deste produto.");
        }

        if (produto.Estoque == null)
        {
            _logger.LogDebug("Criando novo estoque para o produto {ProdutoId}", produtoId);
            produto.Estoque = new Estoque
            {
                ProdutoId = produtoId,
                Quantidade = estoqueDto.Quantidade,
                QuantidadeMinima = estoqueDto.QuantidadeMinima,
                QuantidadeMaxima = estoqueDto.QuantidadeMaxima
            };

            await _dbContext.AtualizarProdutoAsync(produto);
            _logger.LogInformation("Novo estoque criado para o produto {ProdutoId}", produtoId);
        }
        else
        {
            _logger.LogDebug("Atualizando estoque existente para o produto {ProdutoId}", produtoId);
            produto.Estoque.Quantidade = estoqueDto.Quantidade;
            produto.Estoque.QuantidadeMinima = estoqueDto.QuantidadeMinima;
            produto.Estoque.QuantidadeMaxima = estoqueDto.QuantidadeMaxima;

            await _dbContext.AtualizarProdutoAsync(produto);
            _logger.LogInformation("Estoque atualizado para o produto {ProdutoId}", produtoId);
        }

        return new EstoqueDto
        {
            Id = produto.Estoque.Id,
            ProdutoId = produto.Estoque.ProdutoId,
            Quantidade = produto.Estoque.Quantidade,
            QuantidadeMinima = produto.Estoque.QuantidadeMinima,
            QuantidadeMaxima = produto.Estoque.QuantidadeMaxima,
            DataCriacao = produto.Estoque.DataCriacao,
            DataAtualizacao = produto.Estoque.DataAtualizacao
        };
    }
}
