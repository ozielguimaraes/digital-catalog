using MeuCatalogo.Application.DTOs;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Interfaces;
using MeuCatalogo.Application.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using MeuCatalogo.Application.Infrastructure.Data.Repository;
using MeuCatalogo.Application.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore.Storage;
using System.IO;

namespace MeuCatalogo.Application.Services;

public sealed class ProdutoService : IProdutoService
{
    private readonly ILogger<ProdutoService> _logger;
    private readonly ApplicationDbContext _dbContext;

    public ProdutoService(ApplicationDbContext dbContext, ILogger<ProdutoService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        _logger.LogInformation("Serviço de Produto inicializado");
    }

    public async Task<ApiResponse<IEnumerable<ProdutoDto>>> ObterPorCatalogoIdAsync(Guid catalogoId, string userId)
    {
        _logger.LogInformation("Iniciando {Metodo} para catalogoId: {CatalogoId}, usuarioId: {UsuarioId}",
            nameof(ObterPorCatalogoIdAsync), catalogoId, userId);

        var catalogo = await _dbContext.ObterCatalogoPorIdAsync(catalogoId);
        if (catalogo == null || catalogo.UserId != userId)
        {
            _logger.LogWarning("Acesso não autorizado ao catálogo {CatalogoId} pelo usuário {UsuarioId}",
                catalogoId, userId);
            return ApiResponse<IEnumerable<ProdutoDto>>.Error(ResponseType.Forbidden, "Você não tem permissão para acessar este catálogo.");
        }

        _logger.LogDebug("Buscando produtos para o catálogo {CatalogoId}", catalogoId);
        var produtos = await _dbContext.GetProdutosByCatalogoIdAsync(catalogoId);
        _logger.LogInformation("Encontrados {Quantidade} produtos para o catálogo {CatalogoId}",
            produtos.Count, catalogoId);

        var resultado = produtos.Select(p => new ProdutoDto
        {
            Id = p.Id,
            Nome = p.Nome,
            CategoriaId = p.CategoriaId,
            CategoriaNome = p.Categoria?.Nome ?? "Categoria não encontrada",
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

        return ApiResponse<IEnumerable<ProdutoDto>>.Success(resultado);
    }

    public async Task<ApiResponse<ProdutoDto?>> ObterPorIdAsync(Guid id, string userId)
    {
        _logger.LogInformation("Iniciando {Metodo} para produtoId: {ProdutoId}, usuarioId: {UsuarioId}",
            nameof(ObterPorIdAsync), id, userId);

        var produto = await _dbContext.ObterProdutoComDetalhesAsync(id);
        if (produto == null)
        {
            _logger.LogWarning("Produto não encontrado com id: {ProdutoId}", id);
            return ApiResponse<ProdutoDto?>.Error(ResponseType.NotFound, "Produto não encontrado.");
        }

        _logger.LogDebug("Verificando permissões para o produto {ProdutoId}, catalogoId: {CatalogoId}",
            id, produto.CatalogoId);
        var catalogo = await _dbContext.ObterCatalogoPorIdAsync(produto.CatalogoId);
        if (catalogo == null || catalogo.UserId != userId)
        {
            _logger.LogWarning("Acesso não autorizado ao produto {ProdutoId} pelo usuário {UsuarioId}",
                id, userId);
            return ApiResponse<ProdutoDto?>.Error(ResponseType.Forbidden,"Você não tem permissão para acessar este produto.");
        }

        _logger.LogInformation("Retornando detalhes do produto {ProdutoId}", id);
        var dto = new ProdutoDto
        {
            Id = produto.Id,
            Nome = produto.Nome,
            CategoriaId = produto.CategoriaId,
            CategoriaNome = produto.Categoria?.Nome ?? "Categoria não encontrada",
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

        return ApiResponse<ProdutoDto?>.Success(dto);
    }

    public async Task<ApiResponse<ProdutoDto>> AdicionarAsync(ProdutoCreateDto produtoDto, string userId)
    {
        _logger.LogInformation("Iniciando {Metodo} para catalogoId: {CatalogoId}, usuarioId: {UsuarioId}",
            nameof(AdicionarAsync), produtoDto.CatalogoId, userId);

        try
        {
            var catalogo = await _dbContext.ObterCatalogoPorIdAsync(produtoDto.CatalogoId);
            if (catalogo == null || catalogo.UserId != userId)
            {
                _logger.LogWarning("Acesso não autorizado para criar produto no catálogo {CatalogoId} pelo usuário {UsuarioId}",
                    produtoDto.CatalogoId, userId);
                return ApiResponse<ProdutoDto>.Error(ResponseType.Forbidden, "Você não tem permissão para adicionar produtos a este catálogo.");
            }

            bool categoriaExiste = await _dbContext.ExisteCategoriaAsync(produtoDto.CategoriaId);
            if (!categoriaExiste)
            {
                _logger.LogWarning("CategoriaId não pode ser vazio ao criar produto para o catálogo {CatalogoId}", produtoDto.CatalogoId);
                return ApiResponse<ProdutoDto>.Error(ResponseType.Validation,"CategoriaId não pode ser vazio.");
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

            // Adicionar estoque se fornecido
            if (produtoDto.Estoque != null)
            {
                _logger.LogDebug("Adicionando informações de estoque para o produto");
                var estoque = new Estoque
                {
                    ProdutoId = produto.Id,
                    Quantidade = produtoDto.Estoque.Quantidade,
                    QuantidadeMinima = produtoDto.Estoque.QuantidadeMinima,
                    QuantidadeMaxima = produtoDto.Estoque.QuantidadeMaxima
                };

                produto.Estoque = estoque;
            }

            // Adicionar produto e estoque em uma única operação
            await _dbContext.AdicionarAsync(produto);
            _logger.LogInformation("Produto criado com sucesso. Id: {ProdutoId}", produto.Id);

            var dto = new ProdutoDto
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

            return ApiResponse<ProdutoDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar produto para o catálogo {CatalogoId}", produtoDto.CatalogoId);
            throw;
        }
    }

    public async Task<ApiResponse<ProdutoDto>> AtualizarAsync(Guid id, ProdutoUpdateDto produtoDto, string userId)
    {
        _logger.LogInformation("Iniciando {Metodo} para produtoId: {ProdutoId}, usuarioId: {UsuarioId}",
            nameof(AtualizarAsync), id, userId);

        var produto = await _dbContext.ObterProdutoPorIdAsync(id);
        if (produto == null)
        {
            _logger.LogWarning("Produto não encontrado com id: {ProdutoId}", id);
            throw new KeyNotFoundException("Produto não encontrado.");
        }

        _logger.LogDebug("Verificando permissões para atualizar o produto {ProdutoId}, catalogoId: {CatalogoId}",
            id, produto.CatalogoId);
        var catalogo = await _dbContext.ObterCatalogoPorIdAsync(produto.CatalogoId);
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

        var dto = new ProdutoDto
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

        return ApiResponse<ProdutoDto>.Success(dto);
    }

    public async Task<ApiResponse<bool>> RemoverAsync(Guid id, string userId)
    {
        _logger.LogInformation("Iniciando {Metodo} para produtoId: {ProdutoId}, usuarioId: {UsuarioId}",
            nameof(RemoverAsync), id, userId);

        var produto = await _dbContext.GetProdutoByIdAsync(id);
        if (produto == null)
        {
            _logger.LogWarning("Produto não encontrado com id: {ProdutoId}", id);
            return ApiResponse<bool>.Error(ResponseType.NotFound, "Produto não encontrado.");
        }

        _logger.LogDebug("Verificando permissões para excluir o produto {ProdutoId}, catalogoId: {CatalogoId}",
            id, produto.CatalogoId);
        var catalogo = await _dbContext.ObterCatalogoPorIdAsync(produto.CatalogoId);
        if (catalogo == null || catalogo.UserId != userId)
        {
            _logger.LogWarning("Acesso não autorizado para excluir o produto {ProdutoId} pelo usuário {UsuarioId}",
                id, userId);
            return ApiResponse<bool>.Error(ResponseType.Forbidden, "Você não tem permissão para excluir este produto.");
        }

        try
        {
            _logger.LogDebug("Excluindo produto {ProdutoId} do banco de dados", id);
            await _dbContext.RemoverProdutoAsync(id);
            _logger.LogInformation("Produto {ProdutoId} excluído do banco com sucesso", id);

            await RemoverImagensDoProdutoAsync(produto.CatalogoId, id);

            return ApiResponse<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir produto {ProdutoId}", id);
            return ApiResponse<bool>.Error(ResponseType.Validation, "Erro interno do servidor ao excluir produto.");
        }
    }

    private async Task RemoverImagensDoProdutoAsync(Guid catalogoId, Guid produtoId)
    {
        try
        {
            var pastaProduto = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "catalogo", catalogoId.ToString(), "produtos", produtoId.ToString());
            
            if (Directory.Exists(pastaProduto))
            {
                _logger.LogInformation("Removendo pasta de imagens do produto {ProdutoId}: {Pasta}", produtoId, pastaProduto);
                
                Directory.Delete(pastaProduto, true);
                _logger.LogInformation("Pasta do produto {ProdutoId} removida com sucesso", produtoId);

                var pastaCatalogo = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "catalogo", catalogoId.ToString());
                if (Directory.Exists(pastaCatalogo))
                {
                    var subpastas = Directory.GetDirectories(pastaCatalogo);
                    if (subpastas.Length == 0)
                    {
                        Directory.Delete(pastaCatalogo);
                        _logger.LogInformation("Pasta do catálogo {CatalogoId} removida (estava vazia)", catalogoId);
                    }
                }
            }
            else
            {
                _logger.LogDebug("Pasta de imagens do produto {ProdutoId} não encontrada: {Pasta}", produtoId, pastaProduto);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover imagens do produto {ProdutoId}", produtoId);
        }
    }

    public async Task<ApiResponse<EstoqueDto>> AtualizarEstoqueAsync(Guid produtoId, EstoqueUpdateDto estoqueDto, string userId)
    {
        _logger.LogInformation("Iniciando {Metodo} para produtoId: {ProdutoId}, usuarioId: {UsuarioId}",
            nameof(AtualizarEstoqueAsync), produtoId, userId);

        var produto = await _dbContext.GetProdutoWithDetailsAsync(produtoId);
        if (produto == null)
        {
            _logger.LogWarning("Produto não encontrado com id: {ProdutoId}", produtoId);
            return ApiResponse<EstoqueDto>.Error(ResponseType.NotFound, "Produto não encontrado.");
        }

        _logger.LogDebug("Verificando permissões para atualizar estoque do produto {ProdutoId}, catalogoId: {CatalogoId}",
            produtoId, produto.CatalogoId);
        var catalogo = await _dbContext.ObterCatalogoPorIdAsync(produto.CatalogoId);
        if (catalogo == null || catalogo.UserId != userId)
        {
            _logger.LogWarning("Acesso não autorizado para atualizar estoque do produto {ProdutoId} pelo usuário {UsuarioId}",
                produtoId, userId);
            return ApiResponse<EstoqueDto>.Error(ResponseType.Forbidden, "Você não tem permissão para atualizar o estoque deste produto.");
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

        return ApiResponse<EstoqueDto>.Success(new EstoqueDto
        {
            Id = produto.Estoque.Id,
            ProdutoId = produto.Estoque.ProdutoId,
            Quantidade = produto.Estoque.Quantidade,
            QuantidadeMinima = produto.Estoque.QuantidadeMinima,
            QuantidadeMaxima = produto.Estoque.QuantidadeMaxima,
            DataCriacao = produto.Estoque.DataCriacao,
            DataAtualizacao = produto.Estoque.DataAtualizacao
        });
    }
}
